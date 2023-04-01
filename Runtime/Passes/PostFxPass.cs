using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public class PostFxPass : RenderPass<PostFxPass.PostProcessingPassData> {
        private readonly BlurUtility blurUtility;
        private readonly Material bloomCombineMaterial;
        private readonly Material colorCorrectionMaterial;
            
        public class PostProcessingPassData {
            public TextureHandle FinalColorTex;
            public PostFxSettings PostFxSettings;

            public int BloomIterations;
            public TextureHandle[] BloomPyramid;
            public TextureHandle[] BloomPyramidTemp;
        }

        private static readonly int
            bloomSource1Id = Shader.PropertyToID("_Source1"),
            bloomSource2Id = Shader.PropertyToID("_Source2");
        
        
        public PostFxPass(Retrolight pipeline) : base(pipeline) {
            blurUtility = new BlurUtility(BlurUtility.BlurType.Box3);
            bloomCombineMaterial = CoreUtils.CreateEngineMaterial(shaderBundle.BloomCombineShader);
            colorCorrectionMaterial = CoreUtils.CreateEngineMaterial(shaderBundle.ColorCorrectionShader);
        }


        protected override string PassName => "Post Processing Pass";

        public void Run(TextureHandle finalColorTex, PostFxSettings settings) {
            using var builder = CreatePass(out var passData);
            passData.FinalColorTex = builder.ReadWriteTexture(finalColorTex);
            passData.PostFxSettings = settings;
            
            RunBloom(builder, passData);
        }

        private void RunBloom(
            RenderGraphBuilder builder, 
            PostProcessingPassData passData
        ) {
            var bloom = passData.PostFxSettings.Bloom;
            var bloomTexDesc = TextureUtility.ColorTex(new Vector2(0.5f, 0.5f));

            var bloomIterations = 3;
            
            passData.BloomPyramid = new TextureHandle[bloomIterations]; //todo: reset these
            passData.BloomPyramidTemp = new TextureHandle[bloomIterations];
            int i = 0; //todo: this is for later checking against minimum resolution
            for (; i < bloomIterations/*bloom.MaxIterations*/; i++) {
                bloomTexDesc.name = "BloomTex" + i;
                passData.BloomPyramid[i] = builder.CreateTransientTexture(bloomTexDesc);
                bloomTexDesc.name = "BloomTempTex" + i;
                passData.BloomPyramidTemp[i] = builder.CreateTransientTexture(bloomTexDesc);
                bloomTexDesc.scale /= 2;
            }
            passData.BloomIterations = i; 
        }

        protected override void Render(PostProcessingPassData passData, RenderGraphContext context) {
            RenderBloom(passData, context);
        }


        private void RenderBloom(PostProcessingPassData passData, RenderGraphContext context) {
            TextureHandle source = passData.FinalColorTex;
            for (int i = 0; i < passData.BloomIterations; i++) {
                blurUtility.Blur(context.cmd, source, passData.BloomPyramid[i], passData.BloomPyramidTemp[i]);
                source = passData.BloomPyramid[i];
            }
            
            var bloomCombineProps = new MaterialPropertyBlock();
            for (int i = passData.BloomIterations - 2; i >= 0; i--) {
                bloomCombineProps.SetTexture(bloomSource1Id, passData.BloomPyramid[i + 1]);
                bloomCombineProps.SetTexture(bloomSource2Id, passData.BloomPyramid[i]);
                CoreUtils.DrawFullScreen(
                    context.cmd, bloomCombineMaterial, 
                    passData.BloomPyramidTemp[i], bloomCombineProps
                );
            }
            Blitter.BlitCameraTexture(context.cmd, passData.BloomPyramidTemp[0], passData.FinalColorTex, 0, true);
        }

        public override void Dispose() {
            CoreUtils.Destroy(bloomCombineMaterial);
            CoreUtils.Destroy(colorCorrectionMaterial);
        }
    }
}