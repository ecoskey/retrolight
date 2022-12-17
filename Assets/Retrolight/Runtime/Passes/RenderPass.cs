using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public abstract class RenderPass<T> where T : class, new() {
        private readonly RetrolightPipeline pipeline;

        protected RenderGraph renderGraph => pipeline.RenderGraph;
        protected ShaderBundle shaderBundle => pipeline.ShaderBundle;
        protected Camera camera => pipeline.FrameData.Camera;
        protected CullingResults cull => pipeline.FrameData.Cull;
        protected RTHandleProperties rtHandleProperties => pipeline.FrameData.RTHandleProperties;

        protected RenderPass(RetrolightPipeline pipeline) { this.pipeline = pipeline; }
        
        public abstract string PassName { get; }
        protected abstract void Render(T passData, RenderGraphContext context);

        protected RenderGraphBuilder InitPass(out T passData) {
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

        protected TextureHandle CreateDepthTex(string name = TextureUtility.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtility.DepthTex(name));

        protected TextureHandle CreateDepthTex(Vector2 scale, string name = TextureUtility.DefaultDepthTexName) =>
            renderGraph.CreateTexture(TextureUtility.DepthTex(scale, name));
    }
}