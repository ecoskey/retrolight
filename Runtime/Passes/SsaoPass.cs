
using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using Util;
using static Unity.Mathematics.math;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Passes {
    public class SsaoPass : RenderPass {
        private readonly int ssaoKernel;

        public SsaoPass(Retrolight retrolight) : base(retrolight) {
            ssaoKernel = ShaderBundle.Instance.SsaoShader.FindKernel("SSAO");
        }
        
        private class SsaoPassData {
            public TextureHandle SsaoTex;
            public GBuffer GBuffer;
        }
        
        public TextureHandle Run(GBuffer gbuffer) {
            var builder = AddRenderPass<SsaoPassData>("SSAO Pass", out var passData, Render);

            //builder.AllowPassCulling(false); //todo: remove

            var ssaoTexDesc = TextureUtils.ColorTex(
                1, 
                GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.RHalf, TextureUtils.IsSrgb),
                "SSAO Tex"
            );
            ssaoTexDesc.clearBuffer = false;
            //ssaoTexDesc.clearColor = Color.red;
            ssaoTexDesc.enableRandomWrite = true;

            //gbuffer.UseAll(builder, AccessFlags.Read);

            //passData.SsaoTex = CreateUseTex(builder, ssaoTexDesc, AccessFlags.Write);
            passData.SsaoTex = builder.WriteTexture(renderGraph.CreateTexture(ssaoTexDesc));
            return passData.SsaoTex;
        }

        private void Render(SsaoPassData passData, RenderGraphContext ctx) {
            var ssaoShader = ShaderBundle.Instance.SsaoShader;
            ctx.cmd.SetComputeTextureParam(ssaoShader, ssaoKernel, "SsaoTex", passData.SsaoTex);
            
            ctx.cmd.SetComputeMatrixParam(ssaoShader, "unity_MatrixV", camera.worldToCameraMatrix);

            var matrixVp = (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix);
            ctx.cmd.SetComputeMatrixParam(ssaoShader, "unity_MatrixVP", matrixVp);
            ctx.cmd.SetComputeMatrixParam(ssaoShader, "unity_MatrixInvVP", matrixVp.inverse);
            ctx.cmd.SetComputeVectorParam(
                ssaoShader, "_WorldSpaceCameraPos", camera.transform.position
            );
            
            ctx.cmd.DispatchCompute(
                ssaoShader, ssaoKernel, 
                MathUtils.NextMultipleOf(viewportParams.PixelCount.x, Constants.MediumTile),
                MathUtils.NextMultipleOf(viewportParams.PixelCount.y, Constants.MediumTile),
                1 
            );
        }
    }
}