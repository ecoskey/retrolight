using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Runtime.Passes {
    public class GBufferPass : RenderPass<GBufferPass.GBufferPassData> {
        private static readonly ShaderTagId gBufferPassId = new ShaderTagId("RetrolightGBuffer");

        private const string
            albedoTexName = "AlbedoTex",
            depthTexName = "DepthTex",
            normalTexName = "NormalTex",
            attributesTexName = "AttributesTex";

        private static readonly int
            albedoTexId = Shader.PropertyToID(albedoTexName),
            depthTexId = Shader.PropertyToID(depthTexName),
            normalTexId = Shader.PropertyToID(normalTexName),
            attributesTexId = Shader.PropertyToID(attributesTexName);
        
        public class GBufferPassData {
            public GBuffer GBuffer;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(RetrolightPipeline pipeline) : base(pipeline) { }

        public override string PassName => "GBuffer Pass";
        
        public GBuffer Run() {
            using var builder = InitPass(out var passData);
            
            TextureHandle albedo = CreateColorTex(albedoTexName);
            TextureHandle depth = CreateDepthTex(depthTexName);
            TextureHandle normal = CreateColorTex(normalTexName);
            TextureHandle attributes = CreateColorTex(attributesTexName);

            GBuffer gBuffer = new GBuffer(
                builder.UseColorBuffer(albedo, 0), 
                builder.UseDepthBuffer(depth, DepthAccess.Write), 
                builder.UseColorBuffer(normal, 1),
                builder.UseColorBuffer(attributes, 2)
            );
            passData.GBuffer = gBuffer;

            RendererListDesc gBufferRendererDesc  = new RendererListDesc(gBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            passData.GBufferRendererList = builder.UseRendererList(gBufferRendererHandle);
                
            return gBuffer;
        }

        protected override void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.GBufferRendererList);
            
            context.cmd.SetGlobalTexture(albedoTexId, passData.GBuffer.Albedo);
            context.cmd.SetGlobalTexture(depthTexId, passData.GBuffer.Depth);
            context.cmd.SetGlobalTexture(normalTexId, passData.GBuffer.Normal);
            context.cmd.SetGlobalTexture(attributesTexId, passData.GBuffer.Attributes);
        }
    }
}