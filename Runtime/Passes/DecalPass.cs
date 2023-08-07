using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using Data;

namespace Passes {
    public class DecalPass : RenderPass {
        public class DecalPassData {
            public RendererListHandle DecalRendererList;
        }

        public void Run(GBuffer gBuffer) {
            using var builder = AddRenderPass<DecalPassData>("Decal Pass", out var passData, Render);

            //todo: set MRT targets
            //gBuffer.UseAllFrameBuffer(builder, IBaseRenderGraphBuilder.AccessFlags.ReadWrite);

            var decalRendererDesc = new RendererListDesc(Constants.DecalPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            passData.DecalRendererList = renderGraph.CreateRendererList(decalRendererDesc);
            builder.UseRendererList(passData.DecalRendererList);
        }

        private static void Render(DecalPassData passData, RenderGraphContext ctx) {
            ctx.cmd.DrawRendererList(passData.DecalRendererList);
        }

        public DecalPass(Retrolight retrolight) : base(retrolight) { }
    }
}