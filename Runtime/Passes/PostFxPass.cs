using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public class PostFxPass : RenderPass<PostFxPass.PostFxPassData> {
        private readonly ComputeShader bloomShader;
        private readonly ComputeShader colorCorrectionShader;

        private int downsampleFirstK, downsampleK, upsampleK;
        private int colorCorrectionK;

        public class PostFxPassData {
            public TextureHandle FinalColorTex;
            public BloomData BloomData;
            public TextureHandle[] BloomPyramid;
            #if UNITY_EDITOR
            public RendererListHandle PreGizmosRenderer;
            public RendererListHandle PostGizmosRenderer;
            #endif
            public RendererListHandle UiRenderer;
            public RendererListHandle OverlayRenderer;
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

        public void Run(TextureHandle finalColorTex) {
            using var builder = CreatePass(out var passData);
            passData.FinalColorTex = builder.ReadWriteTexture(finalColorTex);
            var stack = VolumeManager.instance.stack;

            RunBloom(
                builder, passData, 
                new BloomData(stack.GetComponent<Overrides.Bloom>(), useHDR, viewportParams.PixelCount)
            );
            
            #if UNITY_EDITOR
            RunGizmos(builder, passData);
            #endif
        }
        
        public struct BloomData {
            public readonly Overrides.Bloom.BloomMode Mode;
            public readonly bool HighQuality;
            public readonly float Intensity;
            public readonly Vector4 ThresholdParams;
            public readonly int Iterations;

            public BloomData(Overrides.Bloom bloom, bool hdr, Vector2Int rtScaledSize) {
                Mode = hdr ? bloom.mode.value : Overrides.Bloom.BloomMode.Additive;
                HighQuality = bloom.highQuality.value;
                Intensity = bloom.intensity.value;
                var threshold = bloom.threshold.value;
                var thresholdKnee = threshold * bloom.knee.value;
                ThresholdParams = new Vector4(
                    threshold, -threshold + thresholdKnee,
                    2 * thresholdKnee, 1f / (4 * thresholdKnee + 1e-5f)
                );

                var size = rtScaledSize / 2;
                var maxIterations = bloom.maxIterations.value;
                var downscaleLimit = bloom.downscaleLimit.value;
                int i = 0; 
                for (; i < maxIterations; i++) {
                    //check if current size goes below safe resolution, if so, break out of loop
                    if (size.x <= downscaleLimit || size.y <= downscaleLimit) break;
                    size /= 2;
                }
                Iterations = i; 
            }
        }

        private void RunBloom(
            RenderGraphBuilder builder, 
            PostFxPassData passData,
            BloomData bloomData
        ) {
            var bloomTexDesc = TextureUtil.ColorTex();
            bloomTexDesc.enableRandomWrite = true;
            passData.BloomPyramid = new TextureHandle[bloomData.Iterations];
            for (int i = 0; i < bloomData.Iterations; i++) {
                bloomTexDesc.scale /= 2;
                bloomTexDesc.name = "BloomTex" + i;
                passData.BloomPyramid[i] = builder.CreateTransientTexture(bloomTexDesc);
            }
        }

        protected override void Render(PostFxPassData passData, RenderGraphContext ctx) {
            #if UNITY_EDITOR
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.PreGizmosRenderer);
            #endif
            if (passData.BloomData is {Iterations: > 0, Intensity: > 0}) RenderBloom(passData, ctx);
            #if UNITY_EDITOR
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.PostGizmosRenderer);
            #endif
        }


        private void RenderBloom(PostFxPassData passData, RenderGraphContext ctx) {
            var bloom = passData.BloomData;
            BloomPass(bloom, ctx.cmd, passData.FinalColorTex, passData.BloomPyramid[0], downsampleFirstK);
            for (int i = 1; i < bloom.Iterations; i++) 
                BloomPass(bloom, ctx.cmd, passData.BloomPyramid[i - 1], passData.BloomPyramid[i], downsampleK);
            for (int i = bloom.Iterations - 1; i > 0; i--) 
                BloomPass(bloom, ctx.cmd, passData.BloomPyramid[i], passData.BloomPyramid[i - 1], upsampleK);
            BloomPass(bloom, ctx.cmd, passData.BloomPyramid[0], passData.FinalColorTex, upsampleK);
        }

        private void BloomPass(BloomData bloomData, CommandBuffer cmd, RTHandle source, RTHandle target, int kernel) {
            Vector4 sourceParams;
            Vector2 rtHandleScale = RTHandles.rtHandleProperties.rtHandleScale;
            Vector2 scaleFactor = source.scaleFactor;
            sourceParams.x = rtHandleScale.x * scaleFactor.x;
            sourceParams.y = rtHandleScale.y * scaleFactor.y;
            Vector2Int scaledSize = source.GetScaledSize();
            sourceParams.z = 1f / scaledSize.x;
            sourceParams.w = 1f / scaledSize.y;

            Vector2Int targetSize = target.GetScaledSize();

            cmd.SetComputeVectorParam(bloomShader, thresholdParamsId, bloomData.ThresholdParams);
            cmd.SetComputeFloatParam(bloomShader, intensityId, bloomData.Intensity);
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

        #if UNITY_EDITOR
        private void RunGizmos(RenderGraphBuilder builder, PostFxPassData passData) {
            passData.PreGizmosRenderer = 
                builder.UseRendererList(renderGraph.CreateGizmoRendererList(camera, GizmoSubset.PreImageEffects));
            passData.PostGizmosRenderer =
                builder.UseRendererList(renderGraph.CreateGizmoRendererList(camera, GizmoSubset.PostImageEffects));
            builder.UseColorBuffer(passData.FinalColorTex, 0);
        }
        #endif
    }
}