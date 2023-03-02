using Retrolight.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Retrolight.Runtime.Passes;
using Retrolight.Util;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Retrolight.Runtime {
    public sealed class Retrolight : RenderPipeline {
        internal RenderGraph RenderGraph { get; private set; }
        internal readonly ShaderBundle ShaderBundle;
        internal readonly int PixelRatio;
        internal FrameData FrameData { get; private set; }

        //render passes
        private SetupPass setupPass;
        private GBufferPass gBufferPass;
        private LightingPass lightingPass;
        private TransparentPass transparentPass;
        private FinalPass finalPass;

        public Retrolight(ShaderBundle shaderBundle, int pixelRatio) {
            //todo: enable SRP batcher, other graphics settings like linear light intensity
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.useScriptableRenderPipelineBatching = true;

            RenderGraph = new RenderGraph("Retrolight Render Graph");
            ShaderBundle = shaderBundle;
            PixelRatio = pixelRatio;

            setupPass = new SetupPass(this);
            gBufferPass = new GBufferPass(this);
            lightingPass = new LightingPass(this);
            transparentPass = new TransparentPass(this);
            finalPass = new FinalPass(this);

            Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
            RTHandles.Initialize(Screen.width / PixelRatio, Screen.height / PixelRatio);
            //RTHandles.ResetReferenceSize(Screen.width / PixelRatio, Screen.height / PixelRatio);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
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
            RTHandles.SetReferenceSize(camera.pixelWidth / PixelRatio, camera.pixelHeight / PixelRatio);
            var viewportParams = new ViewportParams(RTHandles.rtHandleProperties);
            FrameData = new FrameData(camera, cull, viewportParams);
            
            using var snapContext = SnappingUtility.Snap(camera, camera.transform, viewportParams);

            context.SetupCameraProperties(camera);

            CommandBuffer cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
            var renderGraphParams = new RenderGraphParameters {
                scriptableRenderContext = context,
                commandBuffer = cmd,
                currentFrameIndex = Time.frameCount,
            };
            using (RenderGraph.RecordAndExecute(renderGraphParams)) {
                RenderPasses(snapContext.ViewportShift);
            }
            

            if (camera.clearFlags == CameraClearFlags.Skybox) {
                context.DrawSkybox(camera);
            }
            
            context.ExecuteCommandBuffer(cmd);
            #if UNITY_EDITOR
            if (
                SceneView.currentDrawingSceneView is not null &&
                SceneView.currentDrawingSceneView.camera is not null && 
                SceneView.currentDrawingSceneView.camera == camera
            ) {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
            #endif
            
            CommandBufferPool.Release(cmd);
            context.Submit();
        }

        private void RenderPasses(Vector2 viewportShift) {
            setupPass.Run();
            var gBuffer = gBufferPass.Run();
            var finalColorTex = lightingPass.Run(gBuffer);
            transparentPass.Run(gBuffer, finalColorTex);
            //PostProcessPass -> writes to final color buffer after all other shaders
            finalPass.Run(finalColorTex, viewportShift);
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) return;

            setupPass.Dispose();
            gBufferPass.Dispose();
            lightingPass.Dispose();
            transparentPass.Dispose();
            finalPass.Dispose();

            setupPass = null;
            gBufferPass = null;
            lightingPass = null;
            transparentPass = null;
            finalPass = null;

            Blitter.Cleanup();
            RenderGraph.Cleanup();
            RenderGraph = null;
        }
    }
}