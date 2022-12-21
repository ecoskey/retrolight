using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        private static readonly int viewportParamsId = Shader.PropertyToID("ViewportParams");

        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        
        public class SetupPassData { }

        public SetupPass(Retrolight pipeline) : base(pipeline) {
            viewportParamsBuffer = new ConstantBuffer<ViewportParams>();
        }

        protected override string PassName => "Setup Pass";

        public void Run() {
            var builder = CreatePass(out _);
            builder.AllowPassCulling(false); //because we just set shader values, otherwise this pass would be culled :(
        }

        protected override void Render(SetupPassData passData, RenderGraphContext context) {
            viewportParamsBuffer.PushGlobal(context.cmd, viewportParams, viewportParamsId);
        }

        public override void Dispose() => viewportParamsBuffer.Release();
    }
}