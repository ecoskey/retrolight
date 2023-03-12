using System;
using Retrolight.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        private static readonly int viewportParamsId = Shader.PropertyToID("ViewportParams");
        
        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        
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
            
            var lightsDesc = new ComputeBufferDesc(Constants.MaximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            var lightInfo = new LightInfo(lightCount, CreateWriteComputeBuffer(builder, lightsDesc));
            passData.LightInfo = lightInfo;
            
            return lightInfo;
        }

        protected override void Render(SetupPassData passData, RenderGraphContext context) {
            viewportParamsBuffer.PushGlobal(context.cmd, viewportParams, viewportParamsId);
            var lightCount = passData.LightInfo.LightCount;
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                lightCount, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < lightCount; i++) {
                packedLights[i] = new PackedLight(passData.Lights[i], 0);
            }
            
            context.cmd.SetBufferData(passData.LightInfo.LightsBuffer, packedLights, 0, 0, lightCount);
            packedLights.Dispose();
            
            context.cmd.SetGlobalInteger(Constants.LightCountId, lightCount);
            context.cmd.SetGlobalBuffer(Constants.LightBufferId, passData.LightInfo.LightsBuffer);
        }

        public override void Dispose() => viewportParamsBuffer.Release();
    }
}