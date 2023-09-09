using Retrolight.Data;
using Retrolight.Data.Bundles;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Passes {
    public class UtilPasses : RenderPass {
        private readonly ComputeShader hilbertShader;
        private readonly int hilbertKernel;
        private Option<GraphicsBuffer> hilbertBuffer = Option.None<GraphicsBuffer>();

        private readonly Material upscaleMat;
        
        public UtilPasses(Retrolight retrolight) : base(retrolight) {
            hilbertShader = ShaderBundle.Instance.HilbertShader;
            hilbertKernel = hilbertShader.FindKernel("ComputeHilbertIndices");
            
            upscaleMat = CoreUtils.CreateEngineMaterial(ShaderBundle.Instance.UpscaleShader);
        }

        private class HilbertIndicesPassData {
            public BufferHandle IndicesBuf;
        }
        
        public BufferHandle GetHilbertIndices() { 
            if (hilbertBuffer.Enabled) 
                return renderGraph.ImportBuffer(hilbertBuffer.Value);
            
            var builder = AddRenderPass(
                "Compute Hilbert Indices", RenderHilbertIndices, 
                out HilbertIndicesPassData passData
            );
            
            var rawBuf = new GraphicsBuffer(
                GraphicsBuffer.Target.Raw, GraphicsBuffer.UsageFlags.None,
                2048, sizeof(uint)
            );
            hilbertBuffer = Option.Some(rawBuf);

            var wrappedBuf = renderGraph.ImportBuffer(rawBuf);
            builder.WriteBuffer(wrappedBuf);
            passData.IndicesBuf = wrappedBuf;

            return wrappedBuf;
        }

        private void RenderHilbertIndices(HilbertIndicesPassData passData, RenderGraphContext ctx) {
            ctx.cmd.SetComputeBufferParam(hilbertShader, hilbertKernel, "HilbertIndices", passData.IndicesBuf);
            ctx.cmd.DispatchCompute(hilbertShader, hilbertKernel, 4, 4, 1);
        }

        public void PixelPerfectUpscale(
            TextureHandle sourceTex, TextureHandle destinationTex, 
            bool assumeDestFullscreen = false
        ) => renderGraph.AddFullscreenPass(
            "Upscale Pass", upscaleMat, 
            sourceTex, destinationTex, assumeDestFullscreen
        );

        public override void Dispose() {
            if (hilbertBuffer.Enabled) {
                hilbertBuffer.Value.Dispose();
                hilbertBuffer = Option.None<GraphicsBuffer>();
            }
            CoreUtils.Destroy(upscaleMat);
        }
    }
}