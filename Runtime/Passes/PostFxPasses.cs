using Data;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public class PostFxPasses : RenderPass {
        private readonly int downsampleFirstK, downsampleK, upsampleK;
        private readonly int colorCorrectionK;
        
        private class PostFxPassData {
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

        public PostFxPasses(Retrolight pipeline) : base(pipeline) {
            var bloomShader = ShaderBundle.Instance.BloomShader;
            downsampleFirstK = bloomShader.FindKernel("DownsampleFirst");
            downsampleK = bloomShader.FindKernel("Downsample");
            upsampleK = bloomShader.FindKernel("Upsample");
            colorCorrectionK = ShaderBundle.Instance.ColorCorrectionShader.FindKernel("ColorCorrection");
        }

        public void Run(TextureHandle finalColorTex) {
            using var builder = AddRenderPass<PostFxPassData>("Post Processing Pass", out var passData, Render);
            /*using var builder = renderGraph.AddRenderPass(
                "Post Processing Pass", out PostFxPassData passData,
                new ProfilingSampler("PostFxPass")
            );*/
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
            public readonly float4 ThresholdParams;
            public readonly int Iterations;

            public BloomData(Overrides.Bloom bloom, bool hdr, int2 rtScaledSize) {
                Mode = hdr ? bloom.mode.value : Overrides.Bloom.BloomMode.Additive;
                HighQuality = bloom.highQuality.value;
                Intensity = bloom.intensity.value;
                var threshold = bloom.threshold.value;
                var thresholdKnee = threshold * bloom.knee.value;
                ThresholdParams = float4(
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
            var bloomTexDesc = TextureUtils.ColorTex();
            bloomTexDesc.enableRandomWrite = true;
            passData.BloomPyramid = new TextureHandle[bloomData.Iterations];
            for (int i = 0; i < bloomData.Iterations; i++) {
                bloomTexDesc.scale /= 2;
                bloomTexDesc.name = "BloomTex" + i;
                passData.BloomPyramid[i] = builder.CreateTransientTexture(bloomTexDesc);
            }
        }

        private void Render(PostFxPassData passData, RenderGraphContext ctx) {
            #if UNITY_EDITOR
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, passData.PreGizmosRenderer);
            #endif
            if (passData.BloomData is { Iterations: > 0, Intensity: > 0 }) RenderBloom(passData, ctx);
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

        private static void BloomPass(
            BloomData bloomData, CommandBuffer cmd, 
            RTHandle source, RTHandle target, int kernel
        ) {
            var bloomShader = ShaderBundle.Instance.BloomShader;
            
            float4 sourceParams;
            float2 rtHandleScale = float4(RTHandles.rtHandleProperties.rtHandleScale).xy;
            float2 scaleFactor = float2(source.scaleFactor);
            sourceParams.x = rtHandleScale.x * scaleFactor.x;
            sourceParams.y = rtHandleScale.y * scaleFactor.y;
            int2 scaledSize = source.GetScaledSize().AsVector();
            sourceParams.z = 1f / scaledSize.x;
            sourceParams.w = 1f / scaledSize.y;

            int2 targetSize = target.GetScaledSize().AsVector();

            cmd.SetComputeVectorParam(bloomShader, thresholdParamsId, bloomData.ThresholdParams);
            cmd.SetComputeFloatParam(bloomShader, intensityId, bloomData.Intensity);
            cmd.SetComputeVectorParam(bloomShader, sourceParamsId, sourceParams);
            cmd.SetComputeVectorParam(
                bloomShader, targetSizeId, 
                new float4(targetSize, 1f / float2(targetSize))
            );
            cmd.SetComputeTextureParam(bloomShader, kernel, sourceId, source);
            cmd.SetComputeTextureParam(bloomShader, kernel, targetId, target);
            cmd.DispatchCompute(
                bloomShader, kernel, 
                MathUtils.NextMultipleOf(targetSize.x, Constants.MediumTile), 
                MathUtils.NextMultipleOf(targetSize.y, Constants.MediumTile), 1
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