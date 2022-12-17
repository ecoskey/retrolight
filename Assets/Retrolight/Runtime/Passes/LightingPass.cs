using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {
        private const int MaximumLights = 1024;
        private static readonly int
            LightCountId = Shader.PropertyToID("LightCount"),
            LightBufferId = Shader.PropertyToID("Lights"),
            CullingResultsId = Shader.PropertyToID("CullingResults"),
            FinalColorTexId = Shader.PropertyToID("FinalColorTex");

        private readonly int lightCullingKernelId, lightingKernelId;

        public class LightingPassData {
            public NativeArray<VisibleLight> Lights;
            public int LightCount;
            public Vector2Int TileCount;
            public ComputeBufferHandle LightBuffer;
            public ComputeBufferHandle CullingResultsBuffer;
            public Vector4 Resolution;

            public GBuffer GBuffer;
            public TextureHandle FinalColorTex;
        }

        public struct LightingOut {
            public readonly int LightCount;
            public readonly ComputeBufferHandle LightsBuffer;
            public readonly ComputeBufferHandle CullingResultsBuffer;
            public readonly TextureHandle FinalColorTex;

            public LightingOut(
                int lightCount,
                ComputeBufferHandle lightsBuffer,
                ComputeBufferHandle cullingResultsBuffer,
                TextureHandle finalColorTex
            ) {
                LightCount = lightCount;
                LightsBuffer = lightsBuffer;
                CullingResultsBuffer = cullingResultsBuffer;
                FinalColorTex = finalColorTex;
            }
        }
        
        public LightingPass(RetrolightPipeline pipeline) : base(pipeline) {
            lightCullingKernelId = ShaderBundle.LightCullingShader.FindKernel("LightCulling");
            lightingKernelId = ShaderBundle.LightingShader.FindKernel("Lighting");
        }
        
        protected override string PassName => "Lighting Pass";
        
        public LightingOut Run(GBuffer gBuffer) {
            using var builder = CreatePass(out var passData);
            
            passData.Lights = Cull.visibleLights;
            int lightCount = Math.Min(passData.Lights.Length, MaximumLights);
            passData.LightCount = lightCount;
                
            Vector2Int tileCount = new Vector2Int(
                Mathf.CeilToInt(Camera.pixelWidth / 8f),
                Mathf.CeilToInt(Camera.pixelHeight / 8f)
            );
            passData.TileCount = tileCount;

            var lightsDesc = new ComputeBufferDesc(MaximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            var lightsBuffer = RenderGraph.CreateComputeBuffer(lightsDesc);
            lightsBuffer = builder.WriteComputeBuffer(lightsBuffer);
            passData.LightBuffer = lightsBuffer;
                
            var culledLightsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(MaximumLights / 32f) * tileCount.x * tileCount.y, 
                sizeof(uint)
            ) {
                name = "CulledLights", 
                type = ComputeBufferType.Raw,
            };
            var culledLightsBuffer = RenderGraph.CreateComputeBuffer(culledLightsDesc);
            culledLightsBuffer = builder.WriteComputeBuffer(culledLightsBuffer);
            passData.CullingResultsBuffer = culledLightsBuffer;

            passData.GBuffer = gBuffer.ReadAll(builder);

            var finalColorDesc = TextureUtility.ColorTex("FinalColorTex");
            finalColorDesc.enableRandomWrite = true;
            var finalColor = RenderGraph.CreateTexture(finalColorDesc);
            finalColor = builder.WriteTexture(finalColor);
            passData.FinalColorTex = finalColor;

            passData.Resolution = new Vector4(
                Camera.pixelWidth, Camera.pixelHeight, 
                1f / Camera.pixelWidth, 1f / Camera.pixelHeight
            );

            builder.SetRenderFunc<LightingPassData>(Render);
            return new LightingOut(passData.LightCount, lightsBuffer, culledLightsBuffer, finalColor);
        }

        protected override void Render(LightingPassData passData, RenderGraphContext context) {
            NativeArray<VisibleLight> lights = passData.Lights;
            ComputeShader lightCullingShader = ShaderBundle.LightCullingShader;
            ComputeShader lightingShader = ShaderBundle.LightingShader;
            ComputeBuffer lightBuffer = passData.LightBuffer;
            ComputeBuffer cullingResultsBuffer = passData.CullingResultsBuffer;

            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>( //todo: this is a lot of allocation/deallocation each frame
                MaximumLights, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(lights[i]);
            }
            
            context.cmd.SetBufferData(lightBuffer, packedLights, 0, 0, passData.LightCount);
            
            context.cmd.SetGlobalVector("Resolution", passData.Resolution); //todo: move to a setup pass?

            //tiled light culling compute shader
            context.cmd.SetComputeIntParam(lightCullingShader, LightCountId, passData.LightCount);
            context.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, LightBufferId, lightBuffer
            );
            context.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, CullingResultsId, cullingResultsBuffer
            );
            context.cmd.DispatchCompute(
                lightCullingShader, lightCullingKernelId, 
                passData.TileCount.x, passData.TileCount.y, 1
            );

            //lighting calculation compute shader
            context.cmd.SetComputeIntParam(lightCullingShader, LightCountId, passData.LightCount);
            context.cmd.SetComputeBufferParam(
                lightingShader, lightingKernelId, LightBufferId, lightBuffer
            );
            context.cmd.SetComputeBufferParam(
                lightingShader, lightingKernelId, CullingResultsId, cullingResultsBuffer
            );
            context.cmd.SetComputeTextureParam(
                lightingShader, lightingKernelId, FinalColorTexId, passData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                lightingShader, lightingKernelId, 
                passData.TileCount.x, passData.TileCount.y, 1
            );
            
            packedLights.Dispose();
        }
    }
}