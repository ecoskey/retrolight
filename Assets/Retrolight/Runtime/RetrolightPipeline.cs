using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Retrolight.Runtime.Passes;

namespace Retrolight.Runtime {
    public class RetrolightPipeline : RenderPipeline {
        private RenderGraph renderGraph;
        private readonly ShaderBundle shaderBundle;

        private GBufferPass gBufferPass;
        private LightingPass lightingPass;
        private FinalPass finalPass;

        public RetrolightPipeline(ShaderBundle shaderBundle, uint pixelScale) {
            renderGraph = new RenderGraph("Retrolight Render Graph");
            this.shaderBundle = shaderBundle;

            gBufferPass = new GBufferPass(renderGraph);
            lightingPass = new LightingPass(renderGraph, shaderBundle);
            finalPass = new FinalPass(renderGraph);
            
            Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
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
                RenderPasses(camera, cull);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }
        
        private void RenderPasses(Camera camera, CullingResults cull) {
            var gBuffer = gBufferPass.Run(camera, cull);
            lightingPass.Run(camera, cull, gBuffer); //should return light and culling results, and a final color buffer
            //TransparentsPass -> writes to final color buffer
            //PostProcessPass -> writes to final color buffer after all other shaders
            finalPass.Run(camera, gBuffer.Albedo); //todo: use final color buffer as input
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) return;
            
            gBufferPass = null;
            lightingPass = null;
            finalPass = null;

            Blitter.Cleanup();
            renderGraph.Cleanup();
            renderGraph = null;
        }
    }
}