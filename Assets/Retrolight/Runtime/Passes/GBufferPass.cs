using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class GBufferPass {
        private static readonly ShaderTagId GBufferPassId = new ShaderTagId("RetrolightGBuffer");
        private static readonly int
            AlbedoTexId = Shader.PropertyToID("AlbedoTex"),
            DepthTexId = Shader.PropertyToID("DepthTex"),
            NormalTexId = Shader.PropertyToID("NormalTex"),
            AttributesTexId = Shader.PropertyToID("AttributesTex");

        private readonly RenderGraph renderGraph;
        
        class GBufferPassData {
            public GBuffer GBuffer;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }

        public GBuffer Run(Camera camera, CullingResults cull) {
            using var builder = renderGraph.AddRenderPass(
                "Geometry Pass", 
                out GBufferPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            );
            
            TextureHandle albedo = renderGraph.CreateTexture(TextureUtility.ColorTex("AlbedoTex"));
            TextureHandle depth = renderGraph.CreateTexture(TextureUtility.DepthTex());
            TextureHandle normal = renderGraph.CreateTexture(TextureUtility.ColorTex("NormalTex"));
            TextureHandle attributes = renderGraph.CreateTexture(TextureUtility.ColorTex("AttributesTex"));

            GBuffer gBuffer = new GBuffer(
                builder.UseColorBuffer(albedo, 0), 
                builder.UseDepthBuffer(depth, DepthAccess.Write), 
                builder.UseColorBuffer(normal, 1),
                builder.UseColorBuffer(attributes, 2)
            );
            passData.GBuffer = gBuffer;

            RendererListDesc gBufferRendererDesc  = new RendererListDesc(GBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            passData.GBufferRendererList = builder.UseRendererList(gBufferRendererHandle);
                
            builder.SetRenderFunc<GBufferPassData>(Render);
            return gBuffer;
        }

        private static void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.GBufferRendererList);
            
            context.cmd.SetGlobalTexture(AlbedoTexId, passData.GBuffer.Albedo);
            context.cmd.SetGlobalTexture(DepthTexId, passData.GBuffer.Depth);
            context.cmd.SetGlobalTexture(NormalTexId, passData.GBuffer.Normal);
            context.cmd.SetGlobalTexture(AttributesTexId, passData.GBuffer.Attributes);
        }
    }
}