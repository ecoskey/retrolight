using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Retrolight.Passes {
    public class UIPass : RenderPass {
        private static TextureDesc uiTexDesc = TextureUtils.ColorTex(Vector2.one, "UI Tex");
        
        private class UIPassData {
            public RendererListHandle uiRenderer;
            #if UNITY_EDITOR
            public RendererListHandle wireOverlayRenderer;
            public RendererListHandle preFxGizmoRenderer;
            public RendererListHandle postFxGizmoRenderer;
            #endif
        }
        
        public UIPass(Retrolight retrolight) : base(retrolight) { }
        
        public TextureHandle Run() {
            var builder = AddRenderPass("UI Pass", RenderUI, out UIPassData passData);

            var uiRenderer = renderGraph.CreateUIOverlayRendererList(camera);
            builder.UseRendererList(uiRenderer);
            passData.uiRenderer = uiRenderer;
            
            #if UNITY_EDITOR
            var wireOverlayRenderer = renderGraph.CreateWireOverlayRendererList(camera);
            builder.UseRendererList(wireOverlayRenderer);
            passData.wireOverlayRenderer = wireOverlayRenderer;
            
            var preFxGizmoRenderer = renderGraph.CreateGizmoRendererList(camera, GizmoSubset.PreImageEffects);
            builder.UseRendererList(preFxGizmoRenderer);
            passData.preFxGizmoRenderer = preFxGizmoRenderer;
            
            var postFxGizmoRenderer = renderGraph.CreateGizmoRendererList(camera, GizmoSubset.PostImageEffects);
            builder.UseRendererList(postFxGizmoRenderer);
            passData.postFxGizmoRenderer = postFxGizmoRenderer;
            #endif

            var uiTex = builder.UseColorBuffer(renderGraph.CreateTexture(uiTexDesc), 0);
            return uiTex;
        }


        private static void RenderUI(UIPassData passData, RenderGraphContext ctx) {
            #if UNITY_EDITOR
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.wireOverlayRenderer);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.preFxGizmoRenderer);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.postFxGizmoRenderer);
            #endif
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.uiRenderer);
        }
    }
}