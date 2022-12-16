using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class TransparentPass {
        private static readonly ShaderTagId TransparentPassId = new ShaderTagId("RetrolightTransparent");
        
        private readonly RenderGraph renderGraph;
        
        class TransparentPassData {
            public RendererListHandle TransparentRendererList;
        }

        public TransparentPass(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }

        public void Run(Camera camera, CullingResults cull, GBuffer gBuffer, TextureHandle colorTarget) {
            using var builder = renderGraph.AddRenderPass(
                "Transparent Pass", 
                out TransparentPassData passData,
                new ProfilingSampler("Transparent Pass Profiler")
            );
            
            gBuffer.ReadAll(builder);
            builder.UseColorBuffer(colorTarget, 0);

            RendererListDesc transparentRendererDesc = new RendererListDesc(TransparentPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            passData.TransparentRendererList = renderGraph.CreateRendererList(transparentRendererDesc);
            
            builder.AllowRendererListCulling(true);
            builder.SetRenderFunc<TransparentPassData>(Render);
        }

        private void Render(TransparentPassData passData, RenderGraphContext context) {
            context.cmd.DrawRendererList(passData.TransparentRendererList);
        }
    }
}