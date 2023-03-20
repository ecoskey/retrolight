using Retrolight.Data;
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

        public void Run(GBuffer gBuffer, LightInfo lightInfo, LightingData lightingData) {
            using var builder = CreatePass(out var passData);
            
            builder.AllowPassCulling(false);
            builder.AllowRendererListCulling(false);

            gBuffer.ReadAll(builder);
            lightInfo.ReadAll(builder);
            lightingData.ReadAll(builder);

            builder.UseColorBuffer(lightingData.FinalColorTex, 0);
            builder.UseDepthBuffer(gBuffer.Depth, DepthAccess.ReadWrite);

            var transparentRendererDesc = new RendererListDesc(transparentPass, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            var transparentRenderer = renderGraph.CreateRendererList(transparentRendererDesc);
            passData.TransparentRendererList = builder.UseRendererList(transparentRenderer);
        }

        protected override void Render(TransparentPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.TransparentRendererList);
        }
    }
}