using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class DecalPass : RenderPass<DecalPass.DecalPassData> {
        private static readonly ShaderTagId decalPassId = new ShaderTagId("RetrolightDecal");

        public class DecalPassData {
            public RendererListHandle DecalRendererList;
        }

        public DecalPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "Decal Pass";

        public void Run(GBuffer gBuffer) {
            using var builder = CreatePass(out var passData);

            gBuffer.ReadAll(builder);
            builder.UseColorBuffer(gBuffer.Albedo, 0);
            builder.UseDepthBuffer(gBuffer.Depth, DepthAccess.Read);
            builder.UseColorBuffer(gBuffer.Normal, 1);
            builder.UseColorBuffer(gBuffer.Attributes, 2);

            RendererListDesc transparentRendererDesc = new RendererListDesc(decalPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            passData.DecalRendererList = renderGraph.CreateRendererList(transparentRendererDesc);

            builder.AllowRendererListCulling(true);
            builder.SetRenderFunc<DecalPassData>(Render);
        }

        protected override void Render(DecalPassData passData, RenderGraphContext context) {
            context.cmd.DrawRendererList(passData.DecalRendererList);
        }
    }
}