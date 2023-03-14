using Retrolight.Data;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public abstract class RenderPass<T> where T : class, new() {
        private readonly Retrolight pipeline;

        protected RenderGraph renderGraph => pipeline.RenderGraph;
        protected ShaderBundle shaderBundle => pipeline.ShaderBundle;
        protected Camera camera => pipeline.FrameData.Camera;
        protected CullingResults cull => pipeline.FrameData.Cull;
        protected ViewportParams viewportParams => pipeline.FrameData.ViewportParams;
        protected bool usePostFx => pipeline.UsePostFx;
        protected PostFxSettings postFxSettings => pipeline.PostFxSettings;

        protected RenderPass(Retrolight pipeline) {
            this.pipeline = pipeline;
        }

        protected abstract string PassName { get; }
        protected abstract void Render(T passData, RenderGraphContext context);
        public virtual void Dispose() { }

        protected RenderGraphBuilder CreatePass(out T passData) {
            var builder = renderGraph.AddRenderPass(
                PassName, out passData,
                new ProfilingSampler(PassName + " Profiler")
            );
            builder.SetRenderFunc<T>(Render);
            return builder;
        }

        protected TextureHandle CreateColorTex(string name = TextureUtility.DefaultColorTexName) =>
            renderGraph.CreateTexture(TextureUtility.ColorTex(name));

        protected TextureHandle CreateColorTex(Vector2 scale, string name = TextureUtility.DefaultColorTexName) =>
            renderGraph.CreateTexture(TextureUtility.ColorTex(scale, name));

        protected TextureHandle CreateColorTex(
            Vector2 scale, GraphicsFormat format,
            string name = TextureUtility.DefaultColorTexName
        ) => renderGraph.CreateTexture(TextureUtility.ColorTex(scale, format, name));

        protected TextureHandle CreateWriteColorTex(
            RenderGraphBuilder builder,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.WriteTexture(CreateColorTex(name));
        
        protected TextureHandle CreateWriteColorTex(
            RenderGraphBuilder builder, Vector2 scale,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.WriteTexture(CreateColorTex(scale, name));

        protected TextureHandle CreateWriteColorTex(
            RenderGraphBuilder builder, Vector2 scale, GraphicsFormat format,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.WriteTexture(CreateColorTex(scale, format, name));

        protected TextureHandle CreateWriteColorTex(RenderGraphBuilder builder, TextureDesc desc) =>
            builder.WriteTexture(renderGraph.CreateTexture(desc));

        protected TextureHandle CreateUseColorBuffer(
            RenderGraphBuilder builder, int index,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.UseColorBuffer(CreateColorTex(name), index);

        protected TextureHandle CreateUseColorBuffer(
            RenderGraphBuilder builder, int index, Vector2 scale,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.UseColorBuffer(CreateColorTex(scale, name), index);

        protected TextureHandle CreateUseColorBuffer(
            RenderGraphBuilder builder, int index, Vector2 scale, GraphicsFormat format,
            string name = TextureUtility.DefaultColorTexName
        ) => builder.UseColorBuffer(CreateColorTex(scale, format, name), index);

        protected TextureHandle CreateDepthTex(string name = TextureUtility.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtility.DepthTex(name));

        protected TextureHandle CreateDepthTex(Vector2 scale, string name = TextureUtility.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtility.DepthTex(scale, name));

        protected TextureHandle CreateWriteDepthDex(
            RenderGraphBuilder builder,
            string name = TextureUtility.DefaultDepthTexName
        ) => builder.WriteTexture(CreateDepthTex(name));

        protected TextureHandle CreateWriteDepthTex(
            RenderGraphBuilder builder, Vector2 scale,
            string name = TextureUtility.DefaultDepthTexName
        ) => builder.WriteTexture(CreateDepthTex(scale, name));
        
        protected TextureHandle CreateUseDepthBuffer(
            RenderGraphBuilder builder, DepthAccess access,
            string name = TextureUtility.DefaultDepthTexName
        ) => builder.UseDepthBuffer(CreateDepthTex(name), access);

        protected TextureHandle CreateUseDepthBuffer(
            RenderGraphBuilder builder, DepthAccess access, Vector2 scale,
            string name = TextureUtility.DefaultDepthTexName
        ) => builder.UseDepthBuffer(CreateDepthTex(scale, name), access);

        protected ComputeBufferHandle CreateWriteComputeBuffer(RenderGraphBuilder builder, ComputeBufferDesc desc) =>
            builder.WriteComputeBuffer(renderGraph.CreateComputeBuffer(desc));
    }
}