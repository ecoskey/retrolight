using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    public class RetrolightPipeline : RenderPipeline {
        private RenderGraph renderGraph;
        private CameraRenderer renderer;
        public ComputeShader shader;
        private uint pixelScale;

        public RetrolightPipeline(uint pixelScale, ComputeShader shader) {
            renderGraph = new RenderGraph("Retrolight Render Graph");
            renderer = new CameraRenderer(renderGraph, shader, pixelScale);
            this.shader = shader;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            BeginFrameRendering(context, cameras);
            foreach (var camera in cameras) {
                BeginCameraRendering(context, camera);
                renderer.Render(context, camera);
                EndCameraRendering(context, camera);
            }
            renderGraph.EndFrame();
            EndFrameRendering(context, cameras);
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing) {
                renderGraph.Cleanup();
                renderGraph = null;
            }
        }
    }
}