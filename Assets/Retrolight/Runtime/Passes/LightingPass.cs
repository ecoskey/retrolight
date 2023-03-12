using System;
using Retrolight.Data;
using Retrolight.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {

        private static readonly int finalColorTexId = Shader.PropertyToID("FinalColorTex");

        private readonly int lightingKernelId, lightCullingKernelId;

        public class LightingPassData {
            public int lightCount;
            public LightingData LightingData;
        }

        public LightingPass(Retrolight pipeline) : base(pipeline) {
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
        }

        protected override string PassName => "Lighting Pass";
        
        public LightingData Run(GBuffer gBuffer, LightInfo lightInfo) {
            using var builder = CreatePass(out var passData);
            gBuffer.ReadAll(builder);
            lightInfo.ReadAll(builder);

            passData.lightCount = lightInfo.LightCount;

            var finalColorDesc = TextureUtility.ColorTex("FinalColorTex");
            finalColorDesc.enableRandomWrite = true;
            
            var cullingResultsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(Constants.MaximumLights / 32f) * 
                    viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = "Culling Results",
                type = ComputeBufferType.Raw,
            };

            var lightingData = new LightingData(
                CreateWriteColorTex(builder, finalColorDesc), 
                CreateWriteComputeBuffer(builder, cullingResultsDesc)
            );
            passData.LightingData = lightingData;
            
            return lightingData;
        }
        
        protected override void Render(LightingPassData passData, RenderGraphContext context) {
            var tileCount = viewportParams.TileCount;

            context.cmd.SetGlobalBuffer(Constants.CullingResultsId, passData.LightingData.CullingResultsBuffer);

            context.cmd.DispatchCompute(
                shaderBundle.LightCullingShader, lightCullingKernelId, 
                tileCount.x, tileCount.y, 1
            );
            
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixV", camera.worldToCameraMatrix);
            context.cmd.SetComputeMatrixParam(
                shaderBundle.LightingShader, "unity_MatrixInvVP", 
                (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse
            );
            context.cmd.SetComputeVectorParam(
                shaderBundle.LightingShader, "_WorldSpaceCameraPos", camera.transform.position
            );

            context.cmd.SetComputeTextureParam(
                shaderBundle.LightingShader, lightingKernelId, 
                finalColorTexId, passData.LightingData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                shaderBundle.LightingShader, lightingKernelId,
                tileCount.x, tileCount.y, 1
            );
        }
    }
}