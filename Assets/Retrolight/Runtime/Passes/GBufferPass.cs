using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class GBufferPass {
        private static readonly ShaderTagId gBufferPassId = new ShaderTagId("RetrolightGBuffer");

        private readonly RenderGraph renderGraph;
        
        class GBufferPassData {
            public GBuffer gBuffer;
            public RendererListHandle gBufferRendererList;
        }

        public GBufferPass(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }

        public GBuffer Run(Camera camera, CullingResults cull) {
            using (var builder = renderGraph.AddRenderPass(
                "Geometry Pass", 
                out GBufferPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            )) {
                var test = TextureUtility.Color(camera, "Albedo");
                TextureHandle albedo = renderGraph.CreateTexture(test);
                
                TextureHandle depth = renderGraph.CreateTexture(TextureUtility.Depth(camera, "Depth"));
                TextureHandle normal = renderGraph.CreateTexture(TextureUtility.Color(camera, "Normal"));
                TextureHandle attributes = renderGraph.CreateTexture(TextureUtility.Color(camera, "Attributes"));

                GBuffer gBuffer = new GBuffer(
                    builder.UseColorBuffer(albedo, 0), 
                    builder.UseDepthBuffer(depth, DepthAccess.Write), 
                    builder.UseColorBuffer(normal, 1),
                    builder.UseColorBuffer(attributes, 2)
                );
                passData.gBuffer = gBuffer;

                RendererListDesc gBufferRendererDesc  = new RendererListDesc(gBufferPassId, cull, camera) {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    renderQueueRange = RenderQueueRange.opaque
                };
                RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
                passData.gBufferRendererList = builder.UseRendererList(gBufferRendererHandle);
                
                builder.SetRenderFunc<GBufferPassData>(Render);

                return gBuffer;
            }
        }

        private static void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.gBufferRendererList);
        }
    }
}