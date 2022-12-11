using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass {
        public const int MaximumLights = 1024;

        private readonly RenderGraph renderGraph;
        private readonly ShaderBundle shaderBundle;

        class LightingPassData {
            public NativeArray<VisibleLight> Lights;
            public int LightCount;
            public Vector2Int TileCount;
            public ComputeBufferHandle LightsBuffer;
            public ComputeBufferHandle CulledLightsBuffer;

            public GBuffer GBuffer;
            public TextureHandle FinalColor;
        }

        public struct Out {
            public readonly int LightCount;
            public readonly ComputeBufferHandle LightsBuffer;
            public readonly ComputeBufferHandle CulledLightsBuffer;
            public readonly TextureHandle FinalColor;

            public Out(
                int lightCount,
                ComputeBufferHandle lightsBuffer,
                ComputeBufferHandle culledLightsBuffer,
                TextureHandle finalColor
            ) {
                LightCount = lightCount;
                LightsBuffer = lightsBuffer;
                CulledLightsBuffer = culledLightsBuffer;
                FinalColor = finalColor;
            }
        }

        public LightingPass(RenderGraph renderGraph, ShaderBundle shaderBundle) {
            this.renderGraph = renderGraph;
            this.shaderBundle = shaderBundle;
        }

        public Out Run(Camera camera, CullingResults cull, GBuffer gBuffer) {
            using (var builder = renderGraph.AddRenderPass(
                "Lighting Pass", 
                out LightingPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            )) {
                passData.Lights = cull.visibleLights;
                int lightCount = Math.Min(passData.Lights.Length, MaximumLights);
                passData.LightCount = lightCount;

                var lightsDesc = new ComputeBufferDesc(MaximumLights, PackedLight.Stride) {
                    name = "Lights",
                    type = ComputeBufferType.Structured
                };
                var lightsBuffer = renderGraph.CreateComputeBuffer(lightsDesc);
                lightsBuffer = builder.WriteComputeBuffer(lightsBuffer);
                passData.LightsBuffer = lightsBuffer;

                Vector2Int tileCount = new Vector2Int(
                    Mathf.CeilToInt(camera.pixelWidth / 8f),
                    Mathf.CeilToInt(camera.pixelHeight / 8f)
                );
                passData.TileCount = tileCount;
                
                //size shouldn't be relevant for this right??
                var culledLightsDesc = new ComputeBufferDesc(
                    Mathf.CeilToInt(MaximumLights / 32f) * tileCount.x * tileCount.y, 
                    sizeof(uint)
                ) {
                    name = "CulledLights", 
                    type = ComputeBufferType.Raw,
                };
                var culledLightsBuffer = renderGraph.CreateComputeBuffer(culledLightsDesc);
                culledLightsBuffer = builder.WriteComputeBuffer(culledLightsBuffer);
                passData.CulledLightsBuffer = culledLightsBuffer;

                passData.GBuffer = gBuffer.ReadAll(builder);

                var finalColor = renderGraph.CreateTexture(TextureUtility.Color(camera));
                finalColor = builder.WriteTexture(finalColor);
                passData.FinalColor = finalColor;

                builder.EnableAsyncCompute(true);
                builder.SetRenderFunc<LightingPassData>(Render);

                return new Out(passData.LightCount, lightsBuffer, culledLightsBuffer, finalColor);
            }
        }

        private static void Render(LightingPassData passData, RenderGraphContext context) {
            NativeArray<VisibleLight> lights = passData.Lights;
            ComputeBuffer lightsBuffer = passData.LightsBuffer;
            ComputeBuffer culledLightsBuffer = passData.CulledLightsBuffer;

            NativeArray<PackedLight> packedLights = lightsBuffer.BeginWrite<PackedLight>(0, passData.LightCount);
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(lights[i]);
            }
            lightsBuffer.EndWrite<PackedLight>(passData.LightCount);
        }
    }
}