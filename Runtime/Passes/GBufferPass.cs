using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;
using RendererListDesc = UnityEngine.Rendering.RendererUtils.RendererListDesc;

namespace Passes {
    public class GBufferPass : RenderPass<GBufferPass.GBufferPassData> {

        public class GBufferPassData {
            public GBuffer GBuffer;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "GBuffer Pass";

        public GBuffer Run() {
            using var builder = CreatePass(out var passData);

            GBuffer gBuffer = new GBuffer(
                CreateUseColorBuffer(builder, 0, Constants.DiffuseTexName),
                CreateUseColorBuffer(builder, 1, Constants.SpecularTexName),
                CreateUseDepthBuffer(builder, DepthAccess.Write, Constants.DepthTexName),
                CreateUseColorBuffer(builder, 2, Vector2.one, TextureUtil.Packed32Format, Constants.NormalTexName)
            );
            passData.GBuffer = gBuffer;

            var gBufferRendererDesc = new RendererListDesc(Constants.GBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            passData.GBufferRendererList = builder.UseRendererList(gBufferRendererHandle);

            return gBuffer;
        }

        protected override void Render(GBufferPassData passData, RenderGraphContext ctx) {
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.GBufferRendererList);

            var gBuffer = passData.GBuffer;
            ctx.cmd.SetGlobalTexture(Constants.DiffuseTexId, gBuffer.Diffuse);
            ctx.cmd.SetGlobalTexture(Constants.SpecularTexId, gBuffer.Specular);
            ctx.cmd.SetGlobalTexture(Constants.DepthTexId, gBuffer.Depth);
            ctx.cmd.SetGlobalTexture(Constants.NormalTexId, gBuffer.Normal);
        }
    }
}