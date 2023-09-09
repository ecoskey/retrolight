using System;
using Retrolight.Data;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

// ReSharper disable InconsistentNaming

namespace Retrolight.Passes {
    public abstract class RenderPass : IDisposable {
        private readonly Retrolight pipeline;

        protected RenderPass(Retrolight retrolight) {
            pipeline = retrolight;
        }
        
        protected RenderGraph renderGraph => pipeline.RenderGraph;
        protected Camera camera => pipeline.FrameData.Camera;
        protected CullingResults cull => pipeline.FrameData.Cull;
        protected ViewportParams viewportParams => pipeline.FrameData.ViewportParams;
        protected ShadowSettings shadowSettings => pipeline.ShadowSettings;
        protected bool usePostFx => pipeline.FrameData.UsePostFx;
        protected bool useHDR => pipeline.FrameData.UseHDR;

        protected RenderGraphBuilder AddRenderPass<T>(
            string passName, BaseRenderFunc<T, RenderGraphContext> renderFunc, 
            out T passData
        ) where T : class, new() {
            var builder = renderGraph.AddRenderPass(
                passName, out passData,
                new ProfilingSampler(passName + " Profiler")
            );
            builder.SetRenderFunc(renderFunc);
            return builder;
        }

        /*protected IRasterRenderGraphBuilder AddRasterPass<T>(
            string passName, out T passData,
            BaseRenderFunc<T, RasterGraphContext> renderFunc
        ) where T : class, new() {
            var builder = renderGraph.AddRasterRenderPass(
                passName, out passData,
                new ProfilingSampler(passName + " Profiler")
            );
            builder.SetRenderFunc(renderFunc);
            return builder;
        }

        protected IComputeRenderGraphBuilder AddComputePass<T>(
            string passName, out T passData,
            BaseRenderFunc<T, ComputeGraphContext> renderFunc
        ) where T : class, new() {
            var builder = renderGraph.AddComputePass(
                passName, out passData,
                new ProfilingSampler(passName + " Profiler")
            );
            builder.SetRenderFunc(renderFunc);
            return builder;
        }
        
        protected ILowLevelRenderGraphBuilder AddLowLevelPass<T>(
            string passName, out T passData,
            BaseRenderFunc<T, LowLevelGraphContext> renderFunc
        ) where T : class, new() {
            var builder = renderGraph.AddLowLevelPass(
                passName, out passData,
                new ProfilingSampler(passName + " Profiler")
            );
            builder.SetRenderFunc(renderFunc);
            return builder;
        }*/

        protected TextureHandle CreateColorTex(string name = TextureUtils.DefaultColorTexName) =>
            renderGraph.CreateTexture(TextureUtils.ColorTex(name));

        protected TextureHandle CreateColorTex(Vector2 scale, string name = TextureUtils.DefaultColorTexName) =>
            renderGraph.CreateTexture(TextureUtils.ColorTex(scale, name));
        
        protected TextureHandle CreateColorTex(
            Vector2 scale, GraphicsFormat format,
            string name = TextureUtils.DefaultColorTexName
        ) => renderGraph.CreateTexture(TextureUtils.ColorTex(scale, format, name));
        
        protected TextureHandle CreateDepthTex(string name = TextureUtils.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtils.DepthTex(name));

        protected TextureHandle CreateDepthTex(Vector2 scale, string name = TextureUtils.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtils.DepthTex(scale, name));
        
        /*protected TextureHandle CreateUseColorTex(
            IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Write,
            string name = TextureUtils.DefaultColorTexName
        ) => CreateUseTex(builder, TextureUtils.ColorTex(name), accessFlags);

        protected TextureHandle CreateUseColorTex(
            IBaseRenderGraphBuilder builder, float2 scale, AccessFlags accessFlags = AccessFlags.Write,
            string name = TextureUtils.DefaultColorTexName
        ) => CreateUseTex(builder, TextureUtils.ColorTex(scale, name), accessFlags);

        protected TextureHandle CreateUseColorTex(
            IBaseRenderGraphBuilder builder,  float2 scale, GraphicsFormat format, 
            AccessFlags accessFlags = AccessFlags.Write,
            string name = TextureUtils.DefaultColorTexName
        ) => CreateUseTex(builder, TextureUtils.ColorTex(scale, format, name), accessFlags);

        protected TextureHandle CreateUseDepthDex(
            IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Write,
            string name = TextureUtils.DefaultDepthTexName
        ) => CreateUseTex(builder, TextureUtils.DepthTex(name), accessFlags);

        protected TextureHandle CreateUseDepthTex(
            IBaseRenderGraphBuilder builder, float2 scale, AccessFlags accessFlags = AccessFlags.Write,
            string name = TextureUtils.DefaultDepthTexName
        ) => CreateUseTex(builder, TextureUtils.DepthTex(scale, name), accessFlags);
        
        protected TextureHandle CreateUseTex(
            IBaseRenderGraphBuilder builder, TextureDesc desc, 
            AccessFlags accessFlags = AccessFlags.Write
        ) => builder.UseTexture(renderGraph.CreateTexture(desc), accessFlags);

        protected BufferHandle CreateUseBuffer(
            IBaseRenderGraphBuilder builder, BufferDesc desc, AccessFlags accessFlags = AccessFlags.Write
        ) => builder.UseBuffer(renderGraph.CreateBuffer(desc), accessFlags);*/

        public virtual void Dispose() { }
    }
}