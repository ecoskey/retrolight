using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Passes {
    public class ZPrepass : RenderPass {
        public ZPrepass(Retrolight retrolight) : base(retrolight) { }

        private class ZPrepassData {
            public RendererListHandle ZPrepassRenderers;
            public TextureHandle DepthTex;
        }

        public TextureHandle Run() {
            var builder = AddRenderPass("Z Prepass", Render, out ZPrepassData passData);

            var depthTex = builder.UseDepthBuffer(CreateDepthTex(Constants.DepthTexName), DepthAccess.ReadWrite);
            passData.DepthTex = depthTex;
            
            var zPrepassRenderersDesc = new RendererListDesc(Constants.DepthOnlyPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            var zPrepassRenderers = renderGraph.CreateRendererList(zPrepassRenderersDesc);
            builder.UseRendererList(zPrepassRenderers);
            passData.ZPrepassRenderers = zPrepassRenderers;
            
            
            return depthTex;
        }

        private static void Render(ZPrepassData passData, RenderGraphContext ctx) {
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.ZPrepassRenderers);
            ctx.cmd.SetGlobalTexture(Constants.DepthTexId, passData.DepthTex);
        }
    }
}