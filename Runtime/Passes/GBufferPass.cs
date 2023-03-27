using Data;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

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

        protected override void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.GBufferRendererList);

            var gBuffer = passData.GBuffer;
            context.cmd.SetGlobalTexture(Constants.AlbedoTexId, gBuffer.Albedo);
            context.cmd.SetGlobalTexture(Constants.DepthTexId, gBuffer.Depth);
            context.cmd.SetGlobalTexture(Constants.NormalTexId, gBuffer.Normal);
            context.cmd.SetGlobalTexture(Constants.AttributesTexId, gBuffer.Attributes);
        }
    }
}