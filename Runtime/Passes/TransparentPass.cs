using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Passes {
    public class TransparentPass : RenderPass<TransparentPass.TransparentPassData> {
        public class TransparentPassData {
            public RendererListHandle TransparentRendererList;
        }

        public TransparentPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "Transparent Pass";

        private Material testMaterial = CoreUtils.CreateEngineMaterial("Hidden/TestBlit");

        public void Run(GBuffer gBuffer, LightInfo lightInfo, LightingData lightingData) {
            using var builder = CreatePass(out var passData);
            
            //builder.AllowPassCulling(false);
            //builder.AllowRendererListCulling(false);

            gBuffer.ReadAll(builder);
            lightInfo.ReadAll(builder);
            lightingData.ReadAll(builder);

            builder.UseColorBuffer(lightingData.FinalColorTex, 0);
            
            var transparentRendererDesc = new RendererListDesc(Constants.TransparentPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            var transparentRenderer = renderGraph.CreateRendererList(transparentRendererDesc);
            passData.TransparentRendererList = builder.UseRendererList(transparentRenderer);
        }

        protected override void Render(TransparentPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.TransparentRendererList);
            //CoreUtils.DrawFullScreen(context.cmd, testMaterial);
        }
    }
}