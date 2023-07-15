using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using Data;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {
        private readonly int lightingKernelId, lightCullingKernelId;

        public class LightingPassData {
            public LightingData LightingData;
            public RendererList SkyboxRenderer;
        }

        public LightingPass(Retrolight pipeline) : base(pipeline) {
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
        }

        protected override string PassName => "Lighting Pass";
        
        public LightingData Run(GBuffer gBuffer, LightInfo lightInfo, RendererList skyboxRenderer) {
            using var builder = CreatePass(out var passData);
            gBuffer.ReadAll(builder);
            lightInfo.ReadAll(builder);
            
            var finalColorDesc = TextureUtil.ColorTex(Constants.FinalColorTexName);
            finalColorDesc.enableRandomWrite = true;

            passData.SkyboxRenderer = skyboxRenderer;
            
            var cullingResultsDesc = new BufferDesc(
                MathUtil.NextMultipleOf(Constants.MaximumLights, Constants.UIntBitSize) * 
                    viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = Constants.LightCullingResultsBufferName,
                target = GraphicsBuffer.Target.Raw
            };

            var lightingData = new LightingData(
                CreateWriteColorTex(builder, finalColorDesc), 
                CreateWriteBuffer(builder, cullingResultsDesc)
            );
            passData.LightingData = lightingData;
            
            return lightingData;
        }
        
        protected override void Render(LightingPassData passData, RenderGraphContext ctx) {
            //CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.SkyboxRenderer);
            
            var tileCount = viewportParams.TileCount;
            ctx.cmd.SetGlobalBuffer(Constants.LightCullingResultsId, passData.LightingData.CullingResultsBuffer);

            ctx.cmd.DispatchCompute(
                shaderBundle.LightCullingShader, lightCullingKernelId, 
                tileCount.x, tileCount.y, 1
            );
            
            ctx.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixV", camera.worldToCameraMatrix);
            ctx.cmd.SetComputeMatrixParam(
                shaderBundle.LightingShader, "unity_MatrixInvVP", 
                (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse
            );
            ctx.cmd.SetComputeVectorParam(
                shaderBundle.LightingShader, "_WorldSpaceCameraPos", camera.transform.position
            );

            ctx.cmd.SetComputeTextureParam(
                shaderBundle.LightingShader, lightingKernelId, 
                Constants.FinalColorTexId, passData.LightingData.FinalColorTex
            );
            ctx.cmd.DispatchCompute(
                shaderBundle.LightingShader, lightingKernelId,
                tileCount.x, tileCount.y, 1
            );
        }
    }
}