using Retrolight.Data;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Passes {
    public class GBufferPass : RenderPass {
        public class GBufferPassData {
            public GBuffer GBuffer;
            public TextureHandle DepthTex;
            public RendererListHandle GBufferRendererList;
        }

        public GBufferPass(Retrolight pipeline) : base(pipeline) { }

        public void Run(out GBuffer gBuffer, out TextureHandle depthTex) {
            using var builder = AddRenderPass("GBuffer Pass", Render, out GBufferPassData passData);
            depthTex = builder.UseDepthBuffer(CreateDepthTex(Constants.DepthTexName), DepthAccess.ReadWrite);
            gBuffer = InternalRun(builder, passData);
        }

        public GBuffer RunWithZPrepass(TextureHandle depthTex) {
            using var builder = AddRenderPass("GBuffer Pass", RenderWithZPrepass, out GBufferPassData passData);
            passData.DepthTex = builder.UseDepthBuffer(depthTex, DepthAccess.Read);
            return InternalRun(builder, passData);
        }

        //sets everything but depth tex
        private GBuffer InternalRun(RenderGraphBuilder builder, GBufferPassData passData) {
            var gBuffer = new GBuffer(
                builder.UseColorBuffer(CreateColorTex(Constants.DiffuseTexName), 0),
                builder.UseColorBuffer(CreateColorTex(Constants.SpecularTexName), 1),
                builder.UseColorBuffer(
                    CreateColorTex(Vector2.one, GraphicsFormat.R16G16_SFloat, Constants.NormalTexName), 2
                )
            );
            passData.GBuffer = gBuffer;

            var gBufferRendererDesc = new RendererListDesc(Constants.GBufferPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
            builder.UseRendererList(gBufferRendererHandle);
            passData.GBufferRendererList = gBufferRendererHandle;

            return gBuffer;
        }

        private static void RenderWithZPrepass(GBufferPassData passData, RenderGraphContext ctx) {
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.GBufferRendererList);

            var gBuffer = passData.GBuffer;
            ctx.cmd.SetGlobalTexture(Constants.DiffuseTexId, gBuffer.Diffuse);
            ctx.cmd.SetGlobalTexture(Constants.SpecularTexId, gBuffer.Specular);
            ctx.cmd.SetGlobalTexture(Constants.NormalTexId, gBuffer.Normal);
        }

        private static void Render(GBufferPassData passData, RenderGraphContext ctx) {
            RenderWithZPrepass(passData, ctx);
            ctx.cmd.SetGlobalTexture(Constants.DepthTexId, passData.DepthTex);
        }
    }
}