using Data;
using static Unity.Mathematics.math;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using Util;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Passes {
    public class GBufferPass : RenderPass {
        public class GBufferPassData {
            public GBuffer GBuffer;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(Retrolight pipeline) : base(pipeline) { }

        public GBuffer Run() {
            using var builder = AddRenderPass<GBufferPassData>("GBuffer Pass", out var passData, Render);

            var gBuffer = new GBuffer(
                builder.UseColorBuffer(CreateColorTex(Constants.DiffuseTexName), 0),
                builder.UseColorBuffer(CreateColorTex(Constants.SpecularTexName), 1),
                builder.UseColorBuffer(
                    CreateColorTex(1, TextureUtils.Packed32Format, Constants.NormalTexName), 2
                ),
                builder.UseDepthBuffer(CreateDepthTex(Constants.DepthTexName), DepthAccess.ReadWrite)
            );//.UseAllFrameBuffer(builder, AccessFlags.Write);
            passData.GBuffer = gBuffer;

            var gBufferRendererDesc = new RendererListDesc(Constants.GBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            builder.UseRendererList(gBufferRendererHandle);
            passData.GBufferRendererList = gBufferRendererHandle;

            return gBuffer;
        }

        private static void Render(GBufferPassData passData, RenderGraphContext ctx) {
            //ctx.cmd.DrawRendererList(passData.GBufferRendererList);
            
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.GBufferRendererList);

            var gBuffer = passData.GBuffer;
            ctx.cmd.SetGlobalTexture(Constants.DiffuseTexId, gBuffer.Diffuse);
            ctx.cmd.SetGlobalTexture(Constants.SpecularTexId, gBuffer.Specular);
            ctx.cmd.SetGlobalTexture(Constants.DepthTexId, gBuffer.Depth);
            ctx.cmd.SetGlobalTexture(Constants.NormalTexId, gBuffer.Normal);
            //^^removed temporarily because compute shaders can manually read Gbuffer as a parameter, and fragment shaders can sample the framebuffer
        }
    }
}