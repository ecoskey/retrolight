using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class FinalPass : RenderPass<FinalPass.FinalPassData> {
        public class FinalPassData {
            public TextureHandle FinalColorTex;
            public TextureHandle CameraTarget;
        }

        public FinalPass(Retrolight pipeline) : base(pipeline) { }

        public override string PassName => "Final Pass";

        public void Run(TextureHandle finalColor) {
            using var builder = CreatePass(out var passData);

            passData.FinalColorTex = builder.ReadTexture(finalColor);
            TextureHandle cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
            passData.CameraTarget = builder.WriteTexture(cameraTarget);
        }

        protected override void Render(FinalPassData passData, RenderGraphContext context) {
            Vector2 scale = rtHandleProperties.rtHandleScale;
            Blitter.BlitCameraTexture(
                context.cmd, passData.FinalColorTex, passData.CameraTarget,
                new Vector4(scale.x, scale.y, 0, 0) //todo: pixel perfect offset bs
            );
        }
    }
}