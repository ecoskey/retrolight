using Data;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Passes {
    public class ForwardPass : RenderPass {
        public ForwardPass(Retrolight retrolight) : base(retrolight) { }
        
        private class TransparentPassData {
            public RendererListHandle OpaqueRenderers;
            public RendererListHandle TransparentRenderers;
        }
        
        public void Run(GBuffer gBuffer, AllocatedLights allocatedLights, BufferHandle culledLights, TextureHandle finalColorTex) {
            using var builder = AddRenderPass<TransparentPassData>("Transparent Pass", out var passData, Render);
            
            //builder.AllowPassCulling(false);
            //builder.AllowRendererListCulling(false);

            //gBuffer.UseAllFrameBuffer(builder, AccessFlags.Read);
            //allocatedLights.UseAll(builder); //todo: separate depth from this, because it's likely to be used as an FBO attachment

            allocatedLights.ReadAll(builder);
            builder.ReadBuffer(culledLights);//builder.UseBuffer(culledLights, AccessFlags.Read); 
            gBuffer.ReadAll(builder);//builder.UseTextureFragment(finalColorTex, 0, AccessFlags.Write);
            //builder.UseTextureFragmentDepth(gBuffer.Depth, AccessFlags.ReadWrite);
            
            //lightingData.UseAll(builder);
            builder.UseColorBuffer(finalColorTex, 0);
            builder.UseDepthBuffer(gBuffer.Depth, DepthAccess.ReadWrite);

            var opaqueRenderersDesc = new RendererListDesc(Constants.ForwardOpaquePassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonOpaque,
                renderQueueRange = RenderQueueRange.opaque
            };
            var opaqueRenderers = renderGraph.CreateRendererList(opaqueRenderersDesc);
            builder.UseRendererList(opaqueRenderers);
            passData.OpaqueRenderers = opaqueRenderers;

            var transparentRenderersDesc = new RendererListDesc(Constants.ForwardTransparentPassId, cull, camera) {
                sortingCriteria = SortingCriteria.CommonTransparent,
                renderQueueRange = RenderQueueRange.transparent
            };
            
            var transparentRenderers = renderGraph.CreateRendererList(transparentRenderersDesc);
            builder.UseRendererList(transparentRenderers);
            passData.TransparentRenderers = transparentRenderers;
        }

        private static void Render(TransparentPassData passData, RenderGraphContext ctx) {
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.OpaqueRenderers);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.TransparentRenderers);
            //CoreUtils.DrawFullScreen(ctx.cmd, testMaterial);
        }
    }
}