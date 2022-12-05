using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Retrolight.Runtime.Passes;

namespace Retrolight.Runtime {
    public class RetrolightPipeline : RenderPipeline {
        private RenderGraph renderGraph;

        public RetrolightPipeline(int pixelScale) {
            renderGraph = new RenderGraph("Retrolight Render Graph");
            //RTHandles.Initialize(Screen.width, Screen.height);
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            //TODO: FIGURE OUT DUMB RTHANDLE SCALE BUG
            /*Debug.Log(RTHandles.maxWidth);
            Debug.Log(RTHandles.maxHeight);*/
            BeginFrameRendering(context, cameras);
            foreach (var camera in cameras) {
                BeginCameraRendering(context, camera);
                RenderCamera(context, camera);
                EndCameraRendering(context, camera);
            }
            renderGraph.EndFrame();
            EndFrameRendering(context, cameras);
        }
        
        private void RenderCamera(ScriptableRenderContext context, Camera camera) {
            ScriptableCullingParameters cullingParams;
            if (!camera.TryGetCullingParameters(out cullingParams)) return;
            CullingResults cull = context.Cull(ref cullingParams);
            
            context.SetupCameraProperties(camera);

            var cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
            var renderGraphParams = new RenderGraphParameters {
                scriptableRenderContext = context, 
                commandBuffer = cmd, 
                currentFrameIndex = Time.frameCount,
            };

            using (renderGraph.RecordAndExecute(renderGraphParams)) 
                RenderPasses(context, camera, cull);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }
        
        private void RenderPasses(ScriptableRenderContext context, Camera camera, CullingResults cull) {
            var gBuffer = GBufferPass.Run(renderGraph, camera, cull);
            BlitPass.Run(renderGraph, gBuffer);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                renderGraph.Cleanup();
                renderGraph = null;
            }
        }
    }
}