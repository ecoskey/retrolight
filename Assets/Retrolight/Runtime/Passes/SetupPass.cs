using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Runtime.Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        public class SetupPassData { }

        public SetupPass(Retrolight pipeline) : base(pipeline) { }

        public override string PassName => "Setup Pass";

        public void Run() {
            var builder = CreatePass(out _); 
            
            //todo: calculate tiling size, resolution and reciprocal resolution
            //todo: find other common setup stuff to include here
        }

        protected override void Render(SetupPassData passData, RenderGraphContext context) {
            //todo: pass resolution, etc. and rthandle properties as shader variables
        }
    }
}