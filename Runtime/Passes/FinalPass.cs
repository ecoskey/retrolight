using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Passes {
    public class FinalPass : RenderPass {
        private class FinalPassData {
            public TextureHandle FinalColorTex;
            public TextureHandle CameraTarget;
            public Vector2 ViewportShift;
        }
        
        public FinalPass(Retrolight retrolight) : base(retrolight) { }

        public void Run(TextureHandle finalColor, Vector2 viewportShift) {
            using var builder = AddRenderPass("Final Pass", Render, out FinalPassData passData);

            passData.FinalColorTex = builder.ReadTexture(finalColor);
            TextureHandle cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
            passData.CameraTarget = builder.WriteTexture(cameraTarget);
            passData.ViewportShift = viewportShift;
        }

        private static void Render(FinalPassData passData, RenderGraphContext ctx) {
            Blitter.BlitCameraTexture(
                ctx.cmd, passData.FinalColorTex, passData.CameraTarget,
                new Vector4(1, 1, passData.ViewportShift.x, passData.ViewportShift.y), 0, false
            );
        }

    }
}