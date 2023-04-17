using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Passes {
    public class FinalPass : RenderPass<FinalPass.FinalPassData> {
        public class FinalPassData {
            public TextureHandle FinalColorTex;
            public TextureHandle CameraTarget;
            public Vector2 ViewportShift;
        }

        public FinalPass(Retrolight pipeline) : base(pipeline) { }

        protected override string PassName => "Final Pass";

        public void Run(TextureHandle finalColor, Vector2 viewportShift) {
            using var builder = CreatePass(out var passData);

            passData.FinalColorTex = builder.ReadTexture(finalColor);
            TextureHandle cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
            passData.CameraTarget = builder.WriteTexture(cameraTarget);
            passData.ViewportShift = viewportShift;
        }

        protected override void Render(FinalPassData passData, RenderGraphContext ctx) {
            Blitter.BlitCameraTexture(
                ctx.cmd, passData.FinalColorTex, passData.CameraTarget,
                new Vector4(1, 1, passData.ViewportShift.x, passData.ViewportShift.y), 0, false
            );
        }
    }
}