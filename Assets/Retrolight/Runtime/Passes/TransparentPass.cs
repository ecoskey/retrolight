using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class TransparentPass : RenderPass<TransparentPass.TransparentPassData> {
        private static readonly ShaderTagId transparentPass = new ShaderTagId("RetrolightTransparent");

        public class TransparentPassData {
            public RendererListHandle TransparentRendererList;
        }

        public TransparentPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "Transparent Pass";

        public void Run(GBuffer gBuffer, TextureHandle colorTarget) {
            using var builder = CreatePass(out var passData);

            gBuffer.ReadAll(builder);
            builder.UseColorBuffer(colorTarget, 0);

            RendererListDesc transparentRendererDesc = new RendererListDesc(transparentPass, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            passData.TransparentRendererList = renderGraph.CreateRendererList(transparentRendererDesc);
        }

        protected override void Render(TransparentPassData passData, RenderGraphContext context) {
            context.cmd.DrawRendererList(passData.TransparentRendererList);
        }
    }
}