using System;
using System.Collections.Generic;
using Data;
using Unity.Collections;
using static Passes.RenderPass;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Passes {
    public class SetupPass : RenderPass { //todo: make not legacy renderpass
        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        private bool viewportBufferAllocated = false;

        public SetupPass(Retrolight retrolight) : base(retrolight) {
            viewportParamsBuffer = new ConstantBuffer<ViewportParams>();
        }
        
        private class SetupPassData { }

        public void Run() {
            using var builder = renderGraph.AddRenderPass<SetupPassData>("Setup Pass", out _, new ProfilingSampler("Setup Pass"));
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<SetupPassData>(Render);
            //builder.AllowGlobalStateModification(true);
        }
        
        private void Render(SetupPassData passData, RenderGraphContext ctx) {
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