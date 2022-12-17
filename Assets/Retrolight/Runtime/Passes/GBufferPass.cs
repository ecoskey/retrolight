using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class GBufferPass : RenderPass<GBufferPass.GBufferPassData> {
        private static readonly ShaderTagId GBufferPassId = new ShaderTagId("RetrolightGBuffer");
        private static readonly int
            AlbedoTexId = Shader.PropertyToID("AlbedoTex"),
            DepthTexId = Shader.PropertyToID("DepthTex"),
            NormalTexId = Shader.PropertyToID("NormalTex"),
            AttributesTexId = Shader.PropertyToID("AttributesTex");
        
        public class GBufferPassData {
            public GBuffer GBuffer;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(RetrolightPipeline pipeline) : base(pipeline) { }

        protected override string PassName => "GBuffer Pass";
        
        public GBuffer Run() {
            using var builder = CreatePass(out var passData);
            
            TextureHandle albedo = RenderGraph.CreateTexture(TextureUtility.ColorTex("AlbedoTex"));
            TextureHandle depth = RenderGraph.CreateTexture(TextureUtility.DepthTex());
            TextureHandle normal = RenderGraph.CreateTexture(TextureUtility.ColorTex("NormalTex"));
            TextureHandle attributes = RenderGraph.CreateTexture(TextureUtility.ColorTex("AttributesTex"));

            GBuffer gBuffer = new GBuffer(
                builder.UseColorBuffer(albedo, 0), 
                builder.UseDepthBuffer(depth, DepthAccess.Write), 
                builder.UseColorBuffer(normal, 1),
                builder.UseColorBuffer(attributes, 2)
            );
            passData.GBuffer = gBuffer;

            RendererListDesc gBufferRendererDesc  = new RendererListDesc(GBufferPassId, Cull, Camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = RenderGraph.CreateRendererList(gBufferRendererDesc);
            passData.GBufferRendererList = builder.UseRendererList(gBufferRendererHandle);
                
            return gBuffer;
        }

        protected override void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.GBufferRendererList);
            
            context.cmd.SetGlobalTexture(AlbedoTexId, passData.GBuffer.Albedo);
            context.cmd.SetGlobalTexture(DepthTexId, passData.GBuffer.Depth);
            context.cmd.SetGlobalTexture(NormalTexId, passData.GBuffer.Normal);
            context.cmd.SetGlobalTexture(AttributesTexId, passData.GBuffer.Attributes);
        }
    }
}