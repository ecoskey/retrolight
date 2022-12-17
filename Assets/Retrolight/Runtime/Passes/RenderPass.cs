using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public abstract class RenderPass<T> where T : class, new() {
        private readonly RetrolightPipeline pipeline;

        protected RenderGraph RenderGraph => pipeline.RenderGraph;
        protected ShaderBundle ShaderBundle => pipeline.ShaderBundle;

        protected Camera Camera => pipeline.FrameData.Camera;
        protected CullingResults Cull => pipeline.FrameData.Cull;
        protected RTHandleProperties RTHandleProperties => pipeline.FrameData.RTHandleProperties;

        protected RenderPass(RetrolightPipeline pipeline) { this.pipeline = pipeline; }

        protected RenderGraphBuilder CreatePass(out T passData) {
            var builder = RenderGraph.AddRenderPass(
                PassName, out passData, 
                new ProfilingSampler(PassName + " Profiler")
            );
            builder.SetRenderFunc<T>(Render);
            return builder;
        }
        
        protected abstract string PassName { get; }
        protected abstract void Render(T passData, RenderGraphContext context);
    }
}