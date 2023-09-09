using Retrolight.Data;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Passes {
    public class SetupPass : RenderPass { //todo: make not legacy renderpass
        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        private bool viewportBufferAllocated = false;

        public SetupPass(Retrolight retrolight) : base(retrolight) {
            viewportParamsBuffer = new ConstantBuffer<ViewportParams>();
        }
        
        public void Run() {
            using var builder = AddRenderPass("Setup Pass", Render, out Unit _);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<Unit>(Render);
            //builder.AllowGlobalStateModification(true);
        }
        
        private void Render(Unit _, RenderGraphContext ctx) {
            if (!viewportBufferAllocated) {
                viewportBufferAllocated = true;
                viewportParamsBuffer.SetGlobal(ctx.cmd, Constants.ViewportParamsId);
            }
            viewportParamsBuffer.UpdateData(ctx.cmd, viewportParams);
            
            ctx.cmd.SetupCameraProperties(camera);
        }

        public override void Dispose() {
            viewportParamsBuffer.Release();
        }
    }
}