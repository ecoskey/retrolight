using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using Data;

namespace Passes {
    public class DecalPass : RenderPass<DecalPass.DecalPassData> {
        public class DecalPassData {
            public RendererListHandle DecalRendererList;
        }

        public DecalPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "Decal Pass";

        public void Run(GBuffer gBuffer) {
            using var builder = CreatePass(out var passData);

            gBuffer.ReadAll(builder);
            builder.UseColorBuffer(gBuffer.Diffuse, 0);
            builder.UseDepthBuffer(gBuffer.Depth, DepthAccess.Read);
            builder.UseColorBuffer(gBuffer.Normal, 1);
            builder.UseColorBuffer(gBuffer.Specular, 2);

            var decalRendererDesc = new RendererListDesc(Constants.DecalPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            var decalRendererHandle = renderGraph.CreateRendererList(decalRendererDesc);
            passData.DecalRendererList = builder.UseRendererList(decalRendererHandle);
        }

        protected override void Render(DecalPassData passData, RenderGraphContext ctx) {
            ctx.cmd.DrawRendererList(passData.DecalRendererList);
        }
    }
}