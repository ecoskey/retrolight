using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {
        private const int maximumLights = 1024;
        private static readonly int
            lightCountId = Shader.PropertyToID("LightCount"),
            lightBufferId = Shader.PropertyToID("Lights"),
            cullingResultsId = Shader.PropertyToID("CullingResults"),
            finalColorTexId = Shader.PropertyToID("FinalColorTex");

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
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
        }
        
        public override string PassName => "Lighting Pass";
        
        public LightingOut Run(GBuffer gBuffer) {
            using var builder = InitPass(out var passData);
            
            passData.Lights = cull.visibleLights;
            int lightCount = Math.Min(passData.Lights.Length, maximumLights);
            passData.LightCount = lightCount;
                
            Vector2Int tileCount = new Vector2Int(
                Mathf.CeilToInt(camera.pixelWidth / 8f),
                Mathf.CeilToInt(camera.pixelHeight / 8f)
            );
            passData.TileCount = tileCount;

            var lightsDesc = new ComputeBufferDesc(maximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            var lightsBuffer = renderGraph.CreateComputeBuffer(lightsDesc);
            lightsBuffer = builder.WriteComputeBuffer(lightsBuffer);
            passData.LightBuffer = lightsBuffer;
                
            var culledLightsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(maximumLights / 32f) * tileCount.x * tileCount.y, 
                sizeof(uint)
            ) {
                name = "CulledLights", 
                type = ComputeBufferType.Raw,
            };
            var culledLightsBuffer = renderGraph.CreateComputeBuffer(culledLightsDesc);
            culledLightsBuffer = builder.WriteComputeBuffer(culledLightsBuffer);
            passData.CullingResultsBuffer = culledLightsBuffer;

            passData.GBuffer = gBuffer.ReadAll(builder);

            var finalColorDesc = TextureUtility.ColorTex("FinalColorTex");
            finalColorDesc.enableRandomWrite = true;
            var finalColor = builder.WriteTexture(renderGraph.CreateTexture(finalColorDesc));
            passData.FinalColorTex = finalColor;

            //todo: setup resolution, shader properties in a different SetupPass?
            passData.Resolution = new Vector4(
                camera.pixelWidth, camera.pixelHeight, 
                1f / camera.pixelWidth, 1f / camera.pixelHeight
            );

            builder.SetRenderFunc<LightingPassData>(Render);
            return new LightingOut(passData.LightCount, lightsBuffer, culledLightsBuffer, finalColor);
        }
        
        protected override void Render(LightingPassData passData, RenderGraphContext context) {
            NativeArray<VisibleLight> lights = passData.Lights;
            ComputeShader lightCullingShader = shaderBundle.LightCullingShader;
            ComputeShader lightingShader = shaderBundle.LightingShader;
            ComputeBuffer lightBuffer = passData.LightBuffer;
            ComputeBuffer cullingResultsBuffer = passData.CullingResultsBuffer;

            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>( //todo: this is a lot of allocation/deallocation each frame
                maximumLights, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(lights[i]);
            }
            
            context.cmd.SetBufferData(lightBuffer, packedLights, 0, 0, passData.LightCount);
            
            context.cmd.SetGlobalVector("Resolution", passData.Resolution); //todo: move to a setup pass?

            //tiled light culling compute shader
            context.cmd.SetComputeIntParam(lightCullingShader, lightCountId, passData.LightCount);
            context.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, lightBufferId, lightBuffer
            );
            context.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, cullingResultsId, cullingResultsBuffer
            );
            context.cmd.DispatchCompute(
                lightCullingShader, lightCullingKernelId, 
                passData.TileCount.x, passData.TileCount.y, 1
            );

            //lighting calculation compute shader
            context.cmd.SetComputeIntParam(lightCullingShader, lightCountId, passData.LightCount);
            context.cmd.SetComputeBufferParam(
                lightingShader, lightingKernelId, lightBufferId, lightBuffer
            );
            context.cmd.SetComputeBufferParam(
                lightingShader, lightingKernelId, cullingResultsId, cullingResultsBuffer
            );
            context.cmd.SetComputeTextureParam(
                lightingShader, lightingKernelId, finalColorTexId, passData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                lightingShader, lightingKernelId, 
                passData.TileCount.x, passData.TileCount.y, 1
            );
            
            packedLights.Dispose();
        }
    }
}