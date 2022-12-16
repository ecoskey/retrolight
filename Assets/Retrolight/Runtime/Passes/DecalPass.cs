using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class DecalPass {
        private static readonly ShaderTagId DecalPassId = new ShaderTagId("RetrolightDecal");
        
        private readonly RenderGraph renderGraph;
        
        class DecalPassData {
            public RendererListHandle DecalRendererList;
        }

        public DecalPass(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }

        public void Run(Camera camera, CullingResults cull, GBuffer gBuffer) {
            using var builder = renderGraph.AddRenderPass(
                "Decal Pass", 
                out DecalPassData passData,
                new ProfilingSampler("Decal Pass Profiler")
            );
            
            gBuffer.ReadAll(builder);
            builder.UseColorBuffer(gBuffer.Albedo, 0);
            builder.UseDepthBuffer(gBuffer.Depth, DepthAccess.Read);
            builder.UseColorBuffer(gBuffer.Normal, 1);
            builder.UseColorBuffer(gBuffer.Attributes, 2);

            RendererListDesc transparentRendererDesc = new RendererListDesc(DecalPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            passData.DecalRendererList = renderGraph.CreateRendererList(transparentRendererDesc);
            
            builder.AllowRendererListCulling(true);
            builder.SetRenderFunc<DecalPassData>(Render);
        }

        private void Render(DecalPassData passData, RenderGraphContext context) {
            context.cmd.DrawRendererList(passData.DecalRendererList);
        }
    }
}