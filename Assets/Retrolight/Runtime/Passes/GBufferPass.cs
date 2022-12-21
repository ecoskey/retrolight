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

        public GBufferPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "GBuffer Pass";

        public GBuffer Run() {
            using var builder = CreatePass(out var passData);

            GBuffer gBuffer = new GBuffer(
                CreateUseColorBuffer(builder, 0, albedoTexName),
                CreateUseDepthBuffer(builder, DepthAccess.Write, depthTexName),
                CreateUseColorBuffer(builder, 1, normalTexName),
                CreateUseColorBuffer(builder, 2, attributesTexName)
            );
            passData.GBuffer = gBuffer;

            RendererListDesc gBufferRendererDesc = new RendererListDesc(gBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            passData.GBufferRendererList = builder.UseRendererList(gBufferRendererHandle);

            return gBuffer;
        }

        protected override void Render(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.GBufferRendererList);

            var gBuffer = passData.GBuffer;
            context.cmd.SetGlobalTexture(albedoTexId, gBuffer.Albedo);
            context.cmd.SetGlobalTexture(depthTexId, gBuffer.Depth);
            context.cmd.SetGlobalTexture(normalTexId, gBuffer.Normal);
            context.cmd.SetGlobalTexture(attributesTexId, gBuffer.Attributes);
        }
    }
}