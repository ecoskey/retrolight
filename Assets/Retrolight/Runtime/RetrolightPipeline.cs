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
        private GBufferPass gBufferPass;
        private LightingPass lightingPass;
        //private TransparentPass transparentPass;
        private FinalPass finalPass;

        public RetrolightPipeline(ShaderBundle shaderBundle, uint pixelScale) {
            RenderGraph = new RenderGraph("Retrolight Render Graph");
            ShaderBundle = shaderBundle;

            gBufferPass = new GBufferPass(this);
            lightingPass = new LightingPass(this);
            //transparentPass = new TransparentPass(renderGraph);
            finalPass = new FinalPass(this);
            
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
            var gBuffer = gBufferPass.Run();
            var lightingOut = lightingPass.Run(gBuffer);
            //transparentPass.Run(camera, cull, lightingOut.FinalColor);
            //PostProcessPass -> writes to final color buffer after all other shaders
            finalPass.Run(lightingOut.FinalColorTex); //todo: use final color buffer as input
        }

        public override RenderPipelineGlobalSettings defaultSettings { get; }
        protected override void ProcessRenderRequests(ScriptableRenderContext context, Camera camera, List<Camera.RenderRequest> renderRequests) { base.ProcessRenderRequests(context, camera, renderRequests); }

        protected sealed override void Dispose(bool disposing) {
            if (!disposing) return;
            
            gBufferPass = null;
            lightingPass = null;
            //transparentPass = null;
            finalPass = null;

            Blitter.Cleanup();
            RenderGraph.Cleanup();
            RenderGraph = null;
        }
    }
}