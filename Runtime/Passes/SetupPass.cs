using System;
using Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        private bool viewportBufferAllocated = false;
        
        public class SetupPassData {
            public NativeArray<VisibleLight> Lights;
            public LightInfo LightInfo;
        }

        public SetupPass(Retrolight pipeline) : base(pipeline) {
            viewportParamsBuffer = new ConstantBuffer<ViewportParams>();
        }

        protected override string PassName => "Setup Pass";

        public LightInfo Run() {
            using var builder = CreatePass(out var passData);
            builder.AllowPassCulling(false);
            
            passData.Lights = cull.visibleLights;
            
            int lightCount = Math.Min(passData.Lights.Length, Constants.MaximumLights);
            
            var lightsDesc = new BufferDesc(Constants.MaximumLights, PackedLight.Stride) {
                name = Constants.LightBufferName,
                target = GraphicsBuffer.Target.Structured
            };
            var lightInfo = new LightInfo(lightCount, CreateWriteBuffer(builder, lightsDesc));
            passData.LightInfo = lightInfo;
            
            return lightInfo;
        }

        protected override void Render(SetupPassData passData, RenderGraphContext ctx) {
            if (!viewportBufferAllocated) {
                viewportBufferAllocated = true;
                viewportParamsBuffer.SetGlobal(ctx.cmd, Constants.ViewportParamsId);
            }
            viewportParamsBuffer.UpdateData(ctx.cmd, viewportParams);
            
            var lightCount = passData.LightInfo.LightCount;
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                lightCount, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < lightCount; i++) {
                packedLights[i] = new PackedLight(passData.Lights[i], 0);
            }
            
            ctx.cmd.SetBufferData(passData.LightInfo.LightsBuffer, packedLights, 0, 0, lightCount);
            packedLights.Dispose();
            
            ctx.cmd.SetGlobalInteger(Constants.LightCountId, lightCount);
            ctx.cmd.SetGlobalBuffer(Constants.LightBufferId, passData.LightInfo.LightsBuffer);
        }

        public override void Dispose() => viewportParamsBuffer.Release();
    }
}