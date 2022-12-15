using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass {
        public const int MaximumLights = 1024;

        private readonly RenderGraph renderGraph;

        private readonly ComputeShader lightCullingShader;
        private readonly int lightCullingKernelId;

        private readonly ComputeShader lightingShader;
        private readonly int lightingKernelId;
        
        private static readonly int
            LightCountId = Shader.PropertyToID("LightCount"),
            LightBufferId = Shader.PropertyToID("Lights"),
            CullingResultsId = Shader.PropertyToID("CullingResults"),
            FinalColorTexId = Shader.PropertyToID("FinalColorTex");

        class LightingPassData {
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
            public readonly TextureHandle FinalColor;

            public LightingOut(
                int lightCount,
                ComputeBufferHandle lightsBuffer,
                ComputeBufferHandle cullingResultsBuffer,
                TextureHandle finalColor
            ) {
                LightCount = lightCount;
                LightsBuffer = lightsBuffer;
                CullingResultsBuffer = cullingResultsBuffer;
                FinalColor = finalColor;
            }
        }

        public LightingPass(RenderGraph renderGraph, ShaderBundle shaderBundle) {
            this.renderGraph = renderGraph;
            lightCullingShader = shaderBundle.LightCullShader;
            lightCullingKernelId = lightCullingShader.FindKernel("LightCulling");
            lightingShader = shaderBundle.LightingShader;
            lightingKernelId = lightingShader.FindKernel("Lighting");
        }

        public LightingOut Run(Camera camera, CullingResults cull, GBuffer gBuffer) {
            using var builder = renderGraph.AddRenderPass(
                "Lighting Pass", 
                out LightingPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            );
            
            passData.Lights = cull.visibleLights;
            int lightCount = Math.Min(passData.Lights.Length, MaximumLights);
            passData.LightCount = lightCount;
                
            Vector2Int tileCount = new Vector2Int(
                Mathf.CeilToInt(camera.pixelWidth / 8f),
                Mathf.CeilToInt(camera.pixelHeight / 8f)
            );
            passData.TileCount = tileCount;

            var lightsDesc = new ComputeBufferDesc(MaximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            var lightsBuffer = renderGraph.CreateComputeBuffer(lightsDesc);
            lightsBuffer = builder.WriteComputeBuffer(lightsBuffer);
            passData.LightBuffer = lightsBuffer;
                
            var culledLightsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(MaximumLights / 32f) * tileCount.x * tileCount.y, 
                sizeof(uint)
            ) {
                name = "CulledLights", 
                type = ComputeBufferType.Raw,
            };
            var culledLightsBuffer = renderGraph.CreateComputeBuffer(culledLightsDesc);
            culledLightsBuffer = builder.WriteComputeBuffer(culledLightsBuffer);
            passData.CullingResultsBuffer = culledLightsBuffer;

            passData.GBuffer = gBuffer.ReadAll(builder);

            var finalColorDesc = TextureUtility.Color(camera, "FinalColor");
            finalColorDesc.enableRandomWrite = true;
            var finalColor = renderGraph.CreateTexture(finalColorDesc);
            finalColor = builder.WriteTexture(finalColor);
            passData.FinalColorTex = finalColor;

            passData.Resolution = new Vector4(
                camera.pixelWidth, camera.pixelHeight, 
                1f / camera.pixelWidth, 1f / camera.pixelHeight
            );

            builder.EnableAsyncCompute(true);
            builder.SetRenderFunc<LightingPassData>(Render);

            return new LightingOut(passData.LightCount, lightsBuffer, culledLightsBuffer, finalColor);
        }

        private void Render(LightingPassData passData, RenderGraphContext context) {
            NativeArray<VisibleLight> lights = passData.Lights;
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
            
            context.cmd.SetGlobalVector("Resolution", passData.Resolution);

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