using System;
using Retrolight.Data;
using Retrolight.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {
        private const int maximumLights = 1024;
        private const int maxDirectionalShadows = 1;

        private static readonly int
            lightCountId = Shader.PropertyToID("LightCount"),
            lightBufferId = Shader.PropertyToID("Lights"),
            cullingResultsId = Shader.PropertyToID("CullingResults"),
            finalColorTexId = Shader.PropertyToID("FinalColorTex");

        private readonly int lightCullingKernelId, lightingKernelId;

        public class LightingPassData {
            public NativeArray<PackedLight> Lights;
            public int LightCount;
            public Vector2Int TileCount;
            public ComputeBufferHandle LightBuffer;
            public ComputeBufferHandle CullingResultsBuffer;
            public TextureHandle FinalColorTex;
            public TextureHandle ShadowAtlas;
        }

        public LightingPass(Retrolight pipeline) : base(pipeline) {
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
        }

        protected override string PassName => "Lighting Pass";

        public TextureHandle Run(GBuffer gBuffer) {
            using var builder = CreatePass(out var passData);
            gBuffer.ReadAll(builder);
            
            //todo: this is a lot of allocation/deallocation each frame
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                maximumLights, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(cull.visibleLights[i]);
            }
            passData.Lights = packedLights;

            int lightCount = Math.Min(passData.Lights.Length, maximumLights);
            passData.LightCount = lightCount;

            passData.TileCount = viewportParams.TileCount;

            var lightsDesc = new ComputeBufferDesc(maximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            passData.LightBuffer = CreateWriteComputeBuffer(builder, lightsDesc);

            var shadowAtlasDesc = renderGraph.CreateTexture(new TextureDesc(32, 32) { //todo: set width correctly
                colorFormat = GraphicsFormat.None,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true,
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = "Directional Shadow Atlas"
            });
            passData.ShadowAtlas = builder.WriteTexture(shadowAtlasDesc);

            var cullingResultsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(maximumLights / 32f) * viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = "Culling Results",
                type = ComputeBufferType.Raw,
            };
            passData.CullingResultsBuffer = CreateWriteComputeBuffer(builder, cullingResultsDesc);

            var finalColorDesc = TextureUtility.ColorTex("FinalColorTex");
            finalColorDesc.enableRandomWrite = true;
            var finalColorTex = CreateWriteColorTex(builder, finalColorDesc);
            passData.FinalColorTex = finalColorTex;

            return finalColorTex;
        }

        //todo: work on async compute stuff?
        protected override void Render(LightingPassData passData, RenderGraphContext context) {
            //todo: async compute to do shadows and light culling at the same time?
            //SHADOWS
            
            
            // LIGHTS
            //todo: this is a lot of allocation/deallocation each frame
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                maximumLights, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(passData.Lights[i], 0); //todo: VERY BAD FIX THAT ZERO
            }
            context.cmd.SetBufferData(passData.LightBuffer, packedLights, 0, 0, passData.LightCount);
            packedLights.Dispose();

            context.cmd.SetGlobalInt(lightCountId, passData.LightCount);
            context.cmd.SetGlobalBuffer(lightBufferId, passData.LightBuffer);
            context.cmd.SetGlobalBuffer(cullingResultsId, passData.CullingResultsBuffer);
            
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixV", camera.worldToCameraMatrix);
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixInvVP", (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse);
            context.cmd.SetComputeVectorParam(shaderBundle.LightingShader, "_WorldSpaceCameraPos", camera.transform.position);

            context.cmd.DispatchCompute(
                shaderBundle.LightCullingShader, lightCullingKernelId,
                passData.TileCount.x, passData.TileCount.y, 1
            );

            context.cmd.SetComputeTextureParam(
                shaderBundle.LightingShader, lightingKernelId, finalColorTexId, passData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                shaderBundle.LightingShader, lightingKernelId,
                passData.TileCount.x, passData.TileCount.y, 1
            );
        }
    }
}