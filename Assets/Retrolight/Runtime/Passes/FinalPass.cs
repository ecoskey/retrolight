using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class FinalPass {
        private readonly RenderGraph renderGraph;
        
        class FinalPassData {
            public Camera Camera;
            public TextureHandle FinalColorTex;
            public TextureHandle CameraTarget;
        }

        public FinalPass(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }

        public void Run(Camera camera, TextureHandle finalColor) {
            using var builder = renderGraph.AddRenderPass(
                "Final Pass", 
                out FinalPassData passData,
                new ProfilingSampler("Final Pass Profiler")
            );
            
            passData.Camera = camera;
            passData.FinalColorTex = builder.ReadTexture(finalColor);
            TextureHandle cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
            passData.CameraTarget = builder.WriteTexture(cameraTarget);
            builder.SetRenderFunc<FinalPassData>(RenderBlitPass);
        }

        private static void RenderBlitPass(FinalPassData passData, RenderGraphContext context) {
            /*if (passData.Camera.clearFlags == CameraClearFlags.Skybox) {
                context.renderContext.DrawSkybox(passData.Camera);
            }*/
            RTHandle finalColorTex = passData.FinalColorTex;
            Vector2 scale = finalColorTex.scaleFactor;
            Blitter.BlitCameraTexture(
                context.cmd, passData.FinalColorTex,
                passData.CameraTarget, 
                new Vector4(scale.x, scale.y, 0, 0) //todo: pixel perfect offset bs
            );
            context.renderContext.DrawUIOverlay(passData.Camera);
        }
    }
}