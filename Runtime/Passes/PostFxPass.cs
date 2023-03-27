using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Passes {
    public class PostFxPass : RenderPass<PostFxPass.PostProcessingPassData> {
        private readonly Material postProcessingMaterial;

        public class PostProcessingPassData {
            public PostFxSettings PostFxSettings;
        }


        public PostFxPass(Retrolight pipeline) : base(pipeline) {
            postProcessingMaterial = CoreUtils.CreateEngineMaterial("Hidden/RetrolightPostFX");
        }


        protected override string PassName => "Post Processing Pass";

        public void Run(TextureHandle finalColorTex, PostFxSettings settings) {
            
        }

        protected override void Render(PostProcessingPassData passData, RenderGraphContext context) {
            
        }

        public override void Dispose() => CoreUtils.Destroy(postProcessingMaterial);
    }
}