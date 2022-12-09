using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class FinalPass { 
        class FinalPassData {
            public Camera camera;
            public TextureHandle finalColor;
            public TextureHandle cameraTarget;
        }

        public static void Run(RenderGraph renderGraph, Camera camera, TextureHandle finalColor) {
            using (var builder = renderGraph.AddRenderPass(
                "Final Pass", 
                out FinalPassData passData,
                new ProfilingSampler("Final Pass Profiler")
            )) {
                passData.camera = camera;
                passData.finalColor = builder.ReadTexture(finalColor);
                TextureHandle cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
                passData.cameraTarget = builder.WriteTexture(cameraTarget);
                builder.SetRenderFunc<FinalPassData>(RenderBlitPass);
            }
        }

        private static void RenderBlitPass(FinalPassData passData, RenderGraphContext context) {
            if (passData.camera.clearFlags == CameraClearFlags.Skybox) {
                context.renderContext.DrawSkybox(passData.camera);
            }

            Blitter.BlitCameraTexture(
                context.cmd, passData.finalColor, 
                passData.cameraTarget, new Vector4(1, 1, 0, 0) //todo: pixel perfect offset bs
            );
        }
    }
}