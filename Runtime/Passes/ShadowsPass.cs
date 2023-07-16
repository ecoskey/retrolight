using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Passes {
    public class ShadowsPass : RenderPass<ShadowsPass.ShadowsPassData> {
        public class ShadowsPassData {
            public TextureHandle DirectionalShadowAtlas;
            public TextureHandle OtherShadowAtlas;
        }

        public ShadowsPass(Retrolight pipeline) : base(pipeline) { }
        
        protected override string PassName => "Shadows Pass";

        public void RunShadows() {
            
        }

        protected override void Render(ShadowsPassData passData, RenderGraphContext ctx) {
            
        }
    }
}