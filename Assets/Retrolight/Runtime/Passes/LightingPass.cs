using System;
using Retrolight.Data;
using Retrolight.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {
        private const int maximumLights = 1024;
        private const int maxDirectionalShadows = 1;

        private static readonly int
            lightCountId = Shader.PropertyToID("LightCount"),
            lightBufferId = Shader.PropertyToID("Lights"),
            cullingResultsId = Shader.PropertyToID("CullingResults"),
            finalColorTexId = Shader.PropertyToID("FinalColorTex");

        private readonly int lightCullingKernelId, lightingKernelId;

        public class LightingPassData {
            public NativeArray<VisibleLight> Lights;
            public int LightCount;
            public Vector2Int TileCount;
            public ComputeBufferHandle LightBuffer;
            public ComputeBufferHandle CullingResultsBuffer;
            public TextureHandle FinalColorTex;
            //public TextureHandle ShadowAtlas;
        }

        public LightingPass(Retrolight pipeline) : base(pipeline) {
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
        }

        protected override string PassName => "Lighting Pass";

        public TextureHandle Run(GBuffer gBuffer) {
            using var builder = CreatePass(out var passData);
            gBuffer.ReadAll(builder);
            
            int lightCount = Math.Min(passData.Lights.Length, maximumLights);
            passData.LightCount = lightCount;
            passData.Lights = cull.visibleLights;

            passData.TileCount = viewportParams.TileCount;

            var lightsDesc = new ComputeBufferDesc(maximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            passData.LightBuffer = CreateWriteComputeBuffer(builder, lightsDesc);

            /*var shadowAtlasDesc = renderGraph.CreateTexture(new TextureDesc(1024, 1024) { //todo: set width correctly
                colorFormat = GraphicsFormat.None,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true,
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = "Directional Shadow Atlas"
            });
            passData.ShadowAtlas = builder.UseDepthBuffer(shadowAtlasDesc, DepthAccess.Write);*/
            
            var cullingResultsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(maximumLights / 32f) * viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = "Culling Results",
                type = ComputeBufferType.Raw,
            };
            passData.CullingResultsBuffer = CreateWriteComputeBuffer(builder, cullingResultsDesc);

            var finalColorDesc = TextureUtility.ColorTex("FinalColorTex");
            finalColorDesc.enableRandomWrite = true;
            var finalColorTex = CreateWriteColorTex(builder, finalColorDesc);
            passData.FinalColorTex = finalColorTex;

            return finalColorTex;
        }

        //todo: work on async compute stuff?
        protected override void Render(LightingPassData passData, RenderGraphContext context) {
            //todo: async compute to do shadows and light culling at the same time?
            //SHADOWS
            
            //todo: this is a lot of allocation/deallocation each frame
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                passData.LightCount, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(passData.Lights[i], 0);
            }

            //context.cmd.SetGlobalTexture("DirectionalShadowAtlas", passData.ShadowAtlas);

            /*byte directionalShadows = 0;
            for (int i = 0; i < passData.LightCount; i++) {
                var vLight = passData.Lights[i];
                if ( //todo: checking these conditions all over again seems unnecessary??
                    vLight.light.shadows != LightShadows.None && 
                    vLight.light.shadowStrength > 0 && 
                    directionalShadows < maxDirectionalShadows && 
                    //out bounds encapsulates visible shadow casters
                    cull.GetShadowCasterBounds(i, out _)
                ) {
                    packedLights[i] = new PackedLight(vLight, directionalShadows);
                    var shadowSettings = new ShadowDrawingSettings(cull, i);
                    cull.ComputeDirectionalShadowMatricesAndCullingPrimitives( //todo: compute split count, index, and shadow Resolution
                        i, 0, 1, Vector3.zero, 512, 0f,
                        out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                        out ShadowSplitData splitData
                    );
                    shadowSettings.splitData = splitData;

                    Matrix4x4 shadowMatrix = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, Vector2.zero, 0);
                    context.cmd.SetGlobalMatrix("ShadowMatrix", shadowMatrix);
                    context.cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                    context.renderContext.ExecuteCommandBuffer(context.cmd);
                    context.cmd.Clear();
                    context.renderContext.DrawShadows(ref shadowSettings);

                    directionalShadows++;
                } else {
                    packedLights[i] = new PackedLight(vLight, 0);
                }
            }*/

            // LIGHTS
            context.cmd.SetBufferData(passData.LightBuffer, packedLights, 0, 0, passData.LightCount);
            packedLights.Dispose();

            context.cmd.SetGlobalInt(lightCountId, passData.LightCount);
            context.cmd.SetGlobalBuffer(lightBufferId, passData.LightBuffer);
            context.cmd.SetGlobalBuffer(cullingResultsId, passData.CullingResultsBuffer);
            
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixV", camera.worldToCameraMatrix);
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixInvVP", (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse);
            context.cmd.SetComputeVectorParam(shaderBundle.LightingShader, "_WorldSpaceCameraPos", camera.transform.position);

            context.cmd.DispatchCompute(
                shaderBundle.LightCullingShader, lightCullingKernelId,
                passData.TileCount.x, passData.TileCount.y, 1
            );

            context.cmd.SetComputeTextureParam(
                shaderBundle.LightingShader, lightingKernelId, finalColorTexId, passData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                shaderBundle.LightingShader, lightingKernelId,
                passData.TileCount.x, passData.TileCount.y, 1
            );
        }

        
        //Evil
        //Check the Catlike Coding page for reference:
        //https://catlikecoding.com/unity/tutorials/custom-srp/directional-shadows/
        private static Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split) {
            if (SystemInfo.usesReversedZBuffer) {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            
            m.m00 = 0.5f * (m.m00 + m.m30);
            m.m01 = 0.5f * (m.m01 + m.m31);
            m.m02 = 0.5f * (m.m02 + m.m32);
            m.m03 = 0.5f * (m.m03 + m.m33);
            m.m10 = 0.5f * (m.m10 + m.m30);
            m.m11 = 0.5f * (m.m11 + m.m31);
            m.m12 = 0.5f * (m.m12 + m.m32);
            m.m13 = 0.5f * (m.m13 + m.m33);
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            
            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            return m;
        }
    }
}