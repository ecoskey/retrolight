using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Runtime.Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        public class SetupPassData { }

        public SetupPass(Retrolight pipeline) : base(pipeline) { }

        public override string PassName => "Setup Pass";

        public void Run() => InitPass(out _);

        protected override void Render(SetupPassData passData, RenderGraphContext context) {
            throw new System.NotImplementedException();
        }
    }
}