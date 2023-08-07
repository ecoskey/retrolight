using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using static Passes.RenderPass;

namespace Passes {
    public class FinalPass : RenderPass { //todo: still using legacy thingamabopper, 
        private class FinalPassData {
            public TextureHandle FinalColorTex;
            public TextureHandle CameraTarget;
            public float2 ViewportShift;
        }
        
        public FinalPass(Retrolight retrolight) : base(retrolight) { }

        public void Run(TextureHandle finalColor, float2 viewportShift) {
            //using var builder = CreatePass<FinalPassData>("Final Pass", out var passData, Render);
            using var builder = renderGraph.AddRenderPass(
                "Final Pass", out FinalPassData passData,
                new ProfilingSampler("Final Pass Sampler")
            );
            
            builder.SetRenderFunc<FinalPassData>(Render);

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