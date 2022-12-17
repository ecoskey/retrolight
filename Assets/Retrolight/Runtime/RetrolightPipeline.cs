using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Retrolight.Runtime.Passes;

namespace Retrolight.Runtime {
    public class RetrolightPipeline : RenderPipeline {
        public RenderGraph RenderGraph { get; private set; }
        public readonly ShaderBundle ShaderBundle;

        public struct FrameRenderData {
            public readonly Camera Camera;
            public readonly CullingResults Cull;
            public readonly RTHandleProperties RTHandleProperties;

            public FrameRenderData(Camera camera, CullingResults cull, RTHandleProperties rtHandleProperties) {
                Camera = camera;
                Cull = cull;
                RTHandleProperties = rtHandleProperties;
            }
        }

        public FrameRenderData FrameData { get; private set; }

        //render passes
        protected GBufferPass GBufferPass;
        protected LightingPass LightingPass;
        //private TransparentPass transparentPass;
        protected FinalPass FinalPass;

        public RetrolightPipeline(ShaderBundle shaderBundle, uint pixelScale) {
            RenderGraph = new RenderGraph("Retrolight Render Graph");
            ShaderBundle = shaderBundle;

            GBufferPass = new GBufferPass(this);
            LightingPass = new LightingPass(this);
            //transparentPass = new TransparentPass(renderGraph);
            FinalPass = new FinalPass(this);
            
            Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
            RTHandles.Initialize(Screen.width, Screen.height);
        }
        
        protected sealed override void Render(ScriptableRenderContext context, Camera[] cameras) {
            //TODO: FIGURE OUT DUMB RTHANDLE SCALE BUG
            /*Debug.Log(RTHandles.maxWidth);
            Debug.Log(RTHandles.maxHeight);*/
            BeginFrameRendering(context, cameras);
            foreach (var camera in cameras) {
                BeginCameraRendering(context, camera);
                RenderCamera(context, camera);
                EndCameraRendering(context, camera);
            }
            RenderGraph.EndFrame();
            EndFrameRendering(context, cameras);
        }
        
        private void RenderCamera(ScriptableRenderContext context, Camera camera) {
            if (!camera.TryGetCullingParameters(out var cullingParams)) return;
            CullingResults cull = context.Cull(ref cullingParams);
            RTHandles.SetReferenceSize(camera.pixelWidth, camera.pixelHeight);
            FrameData = new FrameRenderData(camera, cull, RTHandles.rtHandleProperties);
            
            context.SetupCameraProperties(camera);

            CommandBuffer cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
            var renderGraphParams = new RenderGraphParameters {
                scriptableRenderContext = context, 
                commandBuffer = cmd, 
                currentFrameIndex = Time.frameCount,
            };

            using (RenderGraph.RecordAndExecute(renderGraphParams)) 
                RenderPasses();

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }
        
        protected virtual void RenderPasses() {
            var gBuffer = GBufferPass.Run();
            var lightingOut = LightingPass.Run(gBuffer);
            //transparentPass.Run(camera, cull, lightingOut.FinalColor);
            //PostProcessPass -> writes to final color buffer after all other shaders
            FinalPass.Run(lightingOut.FinalColorTex); //todo: use final color buffer as input
        }

        protected sealed override void Dispose(bool disposing) {
            if (!disposing) return;
            
            GBufferPass = null;
            LightingPass = null;
            //transparentPass = null;
            FinalPass = null;

            Blitter.Cleanup();
            RenderGraph.Cleanup();
            RenderGraph = null;
        }
    }
}