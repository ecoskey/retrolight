using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public static class LightingPass {
        public const int MaximumLights = 1024;
        
        class LightingPassData {
            public NativeArray<VisibleLight> lights;
            public int lightCount;
            public ComputeBufferHandle lightsBuffer;
            public ComputeBufferHandle culledLightsBuffer;

            public GBuffer gBuffer;
        }

        public static void Run(RenderGraph renderGraph, Camera camera, CullingResults cull, GBuffer gBuffer) {
            using (var builder = renderGraph.AddRenderPass(
                "Lighting Pass", 
                out LightingPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            )) {
                passData.lights = cull.visibleLights;
                passData.lightCount = Math.Min(passData.lights.Length, MaximumLights);

                var lightsDesc = new ComputeBufferDesc(MaximumLights, PackedLight.Stride) {
                    name = "Lights",
                    type = ComputeBufferType.Structured
                };
                var lightsBuffer = renderGraph.CreateComputeBuffer(lightsDesc);
                passData.lightsBuffer = builder.WriteComputeBuffer(lightsBuffer);
                
                //size shouldn't be relevant for this right??
                var culledLightsDesc = new ComputeBufferDesc(Mathf.CeilToInt(MaximumLights / 32f) /* TIMES NUMBER OF TILES */, sizeof(uint)) {
                    name = "CulledLights", 
                    type = ComputeBufferType.Raw,
                };
                var culledLightsBuffer = renderGraph.CreateComputeBuffer(culledLightsDesc);
                passData.culledLightsBuffer = builder.WriteComputeBuffer(culledLightsBuffer);

                passData.gBuffer = gBuffer.ReadAll(builder);

                builder.EnableAsyncCompute(true);
                builder.SetRenderFunc<LightingPassData>(Render);
            }
        }

        private static void Render(LightingPassData passData, RenderGraphContext context) {
            NativeArray<VisibleLight> lights = passData.lights;
            ComputeBuffer lightsBuffer = passData.lightsBuffer;
            ComputeBuffer culledLightsBuffer = passData.culledLightsBuffer;

            NativeArray<PackedLight> packedLights = lightsBuffer.BeginWrite<PackedLight>(0, passData.lightCount);
            for (int i = 0; i < passData.lightCount; i++) {
                packedLights[i] = new PackedLight(lights[i]);
            }
            lightsBuffer.EndWrite<PackedLight>(passData.lightCount);
        }
    }
}