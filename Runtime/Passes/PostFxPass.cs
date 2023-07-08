using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public class PostFxPass : RenderPass<PostFxPass.PostProcessingPassData> {
        private readonly ComputeShader bloomShader;
        private readonly ComputeShader colorCorrectionShader;

        private int downsampleFirstK, downsampleK, upsampleK;
        private int colorCorrectionK;
            
        public class PostProcessingPassData {
            public TextureHandle FinalColorTex;
            public PostFxSettings PostFxSettings;

            public int BloomIterations;
            public TextureHandle[] BloomPyramid;
        }

        private static readonly int
            sourceId = Shader.PropertyToID("Source"),
            targetId = Shader.PropertyToID("Target"),
            thresholdParamsId = Shader.PropertyToID("ThresholdParams"),
            intensityId = Shader.PropertyToID("Intensity"),
            sourceParamsId = Shader.PropertyToID("SourceParams"),
            targetSizeId = Shader.PropertyToID("TargetRes");

        public PostFxPass(Retrolight pipeline) : base(pipeline) {
            bloomShader = shaderBundle.BloomShader;
            colorCorrectionShader = shaderBundle.ColorCorrectionShader;

            downsampleFirstK = bloomShader.FindKernel("DownsampleFirst");
            downsampleK = bloomShader.FindKernel("Downsample");
            upsampleK = bloomShader.FindKernel("Upsample");
            colorCorrectionK = colorCorrectionShader.FindKernel("ColorCorrection");
        }


        protected override string PassName => "Post Processing Pass";

        public void Run(TextureHandle finalColorTex, PostFxSettings settings) {
            using var builder = CreatePass(out var passData);
            passData.FinalColorTex = builder.ReadWriteTexture(finalColorTex);
            passData.PostFxSettings = postFxSettings;
            
            if (settings.BloomSettings.EnableBloom) RunBloom(builder, passData, settings.BloomSettings);
        }

        private void RunBloom(
            RenderGraphBuilder builder, 
            PostProcessingPassData passData,
            BloomSettings bloomSettings
        ) {
            var bloomTexDesc = TextureUtil.ColorTex(new Vector2(0.5f, 0.5f));
            passData.BloomPyramid = new TextureHandle[bloomSettings.MaxIterations];
            int i = 0; 
            for (; i < bloomSettings.MaxIterations; i++) {
                //check if current size goes below safe resolution, if so, break out of loop
                Vector2 scaledSize = RTHandles.rtHandleProperties.currentViewportSize;
                scaledSize.Scale(bloomTexDesc.scale);
                if (scaledSize.x <= bloomSettings.DownscaleLimit || scaledSize.y <= bloomSettings.DownscaleLimit) break;
                
                bloomTexDesc.name = "BloomTex" + i;
                bloomTexDesc.enableRandomWrite = true;
                passData.BloomPyramid[i] = builder.CreateTransientTexture(bloomTexDesc);
                bloomTexDesc.scale /= 2;
            }
            passData.BloomIterations = i; 
        }

        protected override void Render(PostProcessingPassData passData, RenderGraphContext ctx) {
            if (passData.BloomIterations > 0) RenderBloom(passData, ctx);
        }


        private void RenderBloom(PostProcessingPassData passData, RenderGraphContext ctx) {
            if (passData.BloomIterations <= 0) return;
            
            var bloom = passData.PostFxSettings.BloomSettings;
            BloomPass(bloom, ctx.cmd, passData.FinalColorTex, passData.BloomPyramid[0], downsampleFirstK);
            for (int i = 1; i < passData.BloomIterations; i++) 
                BloomPass(bloom, ctx.cmd, passData.BloomPyramid[i - 1], passData.BloomPyramid[i], downsampleK);
            for (int i = passData.BloomIterations - 1; i > 0; i--) 
                BloomPass(bloom, ctx.cmd, passData.BloomPyramid[i], passData.BloomPyramid[i - 1], upsampleK);
            BloomPass(bloom, ctx.cmd, passData.BloomPyramid[0], passData.FinalColorTex, upsampleK);
        }

        private void BloomPass(BloomSettings bloomSettings, CommandBuffer cmd, RTHandle source, RTHandle target, int kernel) {
            Vector4 sourceParams;
            Vector2 rtHandleScale = RTHandles.rtHandleProperties.rtHandleScale;
            Vector2 scaleFactor = source.scaleFactor;
            sourceParams.x = rtHandleScale.x * scaleFactor.x;
            sourceParams.y = rtHandleScale.y * scaleFactor.y;
            Vector2Int scaledSize = source.GetScaledSize();
            sourceParams.z = 1f / scaledSize.x;
            sourceParams.w = 1f / scaledSize.y;

            Vector2Int targetSize = target.GetScaledSize();

            cmd.SetComputeVectorParam(bloomShader, thresholdParamsId, bloomSettings.ThresholdParams);
            cmd.SetComputeFloatParam(bloomShader, intensityId, bloomSettings.Intensity);
            cmd.SetComputeVectorParam(bloomShader, sourceParamsId, sourceParams);
            cmd.SetComputeVectorParam(
                bloomShader, targetSizeId, 
                new Vector4(targetSize.x, targetSize.y, 1f / targetSize.x, 1f / targetSize.y)
            );
            cmd.SetComputeTextureParam(bloomShader, kernel, sourceId, source);
            cmd.SetComputeTextureParam(bloomShader, kernel, targetId, target);
            cmd.DispatchCompute(
                bloomShader, kernel, 
                MathUtil.NextMultipleOf(targetSize.x, Constants.SmallTile), 
                MathUtil.NextMultipleOf(targetSize.y, Constants.SmallTile), 1
            );
        }
    }
}