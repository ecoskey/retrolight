using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Retrolight.Runtime.Passes;

namespace Retrolight.Runtime {
    public class Retrolight : RenderPipeline {
        protected internal RenderGraph RenderGraph { get; private set; }
        internal readonly ShaderBundle ShaderBundle;
        internal FrameData FrameData { get; private set; }

        //render passes
        protected GBufferPass GBufferPass;
        protected LightingPass LightingPass;
        protected TransparentPass TransparentPass;
        protected FinalPass FinalPass;

        public Retrolight(ShaderBundle shaderBundle, uint pixelScale) {
            RenderGraph = new RenderGraph("Retrolight Render Graph");
            ShaderBundle = shaderBundle;

            GBufferPass = new GBufferPass(this);
            LightingPass = new LightingPass(this);
            TransparentPass = new TransparentPass(this);
            FinalPass = new FinalPass(this);

            Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
            RTHandles.Initialize(Screen.width, Screen.height);
        }

        protected sealed override void Render(ScriptableRenderContext context, Camera[] cameras) {
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
            FrameData = new FrameData(camera, cull, RTHandles.rtHandleProperties);

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
            var finalColorTex = LightingPass.Run(gBuffer, Vector2Int.zero); //todo: 
            TransparentPass.Run(gBuffer, finalColorTex);
            //PostProcessPass -> writes to final color buffer after all other shaders
            FinalPass.Run(finalColorTex);
        }

        protected sealed override void Dispose(bool disposing) {
            if (!disposing) return;

            GBufferPass = null;
            LightingPass = null;
            TransparentPass = null;
            FinalPass = null;

            Blitter.Cleanup();
            RenderGraph.Cleanup();
            RenderGraph = null;
        }
    }
}