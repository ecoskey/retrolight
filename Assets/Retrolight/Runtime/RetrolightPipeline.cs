using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    public class RetrolightPipeline : RenderPipeline {
        private RenderGraph renderGraph;
        private CameraRenderer renderer;

        public RetrolightPipeline() {
            renderGraph = new RenderGraph("Retrolight Render Graph");
            renderer = new CameraRenderer(renderGraph);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            foreach (var camera in cameras) {
                renderer.Render(context, camera);
            }
            renderGraph.EndFrame();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                renderGraph.Cleanup();
                renderGraph = null;
            }
        }
    }
}