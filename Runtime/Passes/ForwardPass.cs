using Retrolight.Data;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Passes {
    public class ForwardPass : RenderPass {
        public ForwardPass(Retrolight retrolight) : base(retrolight) { }
        
        private class TransparentPassData {
            public RendererListHandle OpaqueRenderers;
            public RendererListHandle TransparentRenderers;
        }
        
        public void Run(
            GBuffer gBuffer, TextureHandle depthTex, AllocatedLights allocatedLights, 
            BufferHandle culledLights, TextureHandle sceneTex, bool writeDepth
        ) {
            using var builder = AddRenderPass("Transparent Pass", Render, out TransparentPassData passData);
            
            //builder.AllowPassCulling(false);
            //builder.AllowRendererListCulling(false);

            //gBuffer.UseAllFrameBuffer(builder, AccessFlags.Read);
            //allocatedLights.UseAll(builder); //todo: separate depth from this, because it's likely to be used as an FBO attachment

            allocatedLights.ReadAll(builder);
            builder.ReadBuffer(culledLights); //builder.UseBuffer(culledLights, AccessFlags.Read); 
            gBuffer.ReadAll(builder); //builder.UseTextureFragment(finalColorTex, 0, AccessFlags.Write);
            //builder.UseTextureFragmentDepth(gBuffer.Depth, AccessFlags.ReadWrite);
            
            //lightingData.UseAll(builder);
            builder.UseColorBuffer(sceneTex, 0);
            builder.UseDepthBuffer(depthTex, writeDepth ? DepthAccess.ReadWrite : DepthAccess.Read);

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