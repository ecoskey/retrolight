using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
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
                CreateUseColorBuffer(builder, 0, Constants.AlbedoTexName),
                CreateUseDepthBuffer(builder, DepthAccess.Write, Constants.DepthTexName),
                CreateUseColorBuffer(builder, 1, Constants.NormalTexName),
                CreateUseColorBuffer(builder, 2, Constants.AttributesTexName)
            );
            passData.GBuffer = gBuffer;

            RendererListDesc gBufferRendererDesc = new RendererListDesc(Constants.GBufferPassId, cull, camera) {
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
            ctx.cmd.SetGlobalTexture(Constants.AlbedoTexId, gBuffer.Albedo);
            ctx.cmd.SetGlobalTexture(Constants.DepthTexId, gBuffer.Depth);
            ctx.cmd.SetGlobalTexture(Constants.NormalTexId, gBuffer.Normal);
            ctx.cmd.SetGlobalTexture(Constants.AttributesTexId, gBuffer.Attributes);
        }
    }
}