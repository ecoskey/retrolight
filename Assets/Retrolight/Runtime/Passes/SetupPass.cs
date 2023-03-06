using System;
using Retrolight.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class SetupPass : RenderPass<SetupPass.SetupPassData> {
        private static readonly int viewportParamsId = Shader.PropertyToID("ViewportParams");
        
        private readonly ConstantBuffer<ViewportParams> viewportParamsBuffer;
        
        private const int maximumLights = 1024,
                          maxDirectionalShadows = 16,
                          maxOtherShadows = 64;

        private readonly int lightCullingKernelId;


        private readonly int
            lightCountId = Shader.PropertyToID("LightCount"),
            lightBufferId = Shader.PropertyToID("Lights"),
            cullingResultsId = Shader.PropertyToID("CullingResults"),
            directionalShadowAtlasId = Shader.PropertyToID("DirectionalShadowAtlas"),
            otherShadowAtlasId = Shader.PropertyToID("OtherShadowAtlas"),
            directionalShadowMatricesId = Shader.PropertyToID("DirectionalShadowMatrices"),
            otherShadowMatricesId = Shader.PropertyToID("OtherShadowMatrices");
        
        public class SetupPassData {
            public NativeArray<VisibleLight> Lights;
            public int LightCount;
            public LightingData LightingData;
        }

        public SetupPass(Retrolight pipeline) : base(pipeline) {
            viewportParamsBuffer = new ConstantBuffer<ViewportParams>();
            lightCullingKernelId = shaderBundle.LightCullingShader.FindKernel("LightCulling");
        }

        protected override string PassName => "Setup Pass";

        public LightingData Run() {
            using var builder = CreatePass(out var passData);
            
            passData.Lights = cull.visibleLights;
            passData.LightCount = Math.Min(passData.Lights.Length, maximumLights);
            var lightsDesc = new ComputeBufferDesc(maximumLights, PackedLight.Stride) {
                name = "Lights",
                type = ComputeBufferType.Structured
            };
            
            var cullingResultsDesc = new ComputeBufferDesc(
                Mathf.CeilToInt(maximumLights / 32f) * viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = "Culling Results",
                type = ComputeBufferType.Raw,
            };
            
            //todo: set width/height correctly
            var directionalShadowAtlasDesc = renderGraph.CreateTexture(new TextureDesc(1024, 1024) { 
                colorFormat = GraphicsFormat.None,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true,
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = "Directional Shadow Atlas"
            });

            var lightingData = new LightingData(
                CreateWriteComputeBuffer(builder, lightsDesc),
                CreateWriteComputeBuffer(builder, cullingResultsDesc),
                builder.UseDepthBuffer(directionalShadowAtlasDesc, DepthAccess.Write)
            );
            passData.LightingData = lightingData;
            
            return lightingData;
        }

        protected override void Render(SetupPassData passData, RenderGraphContext context) {
            viewportParamsBuffer.PushGlobal(context.cmd, viewportParams, viewportParamsId);
            CullLights(passData, context);
            //RenderShadows(passData, context);
            //RenderLighting(passData, context);
        }

        private void CullLights(SetupPassData passData, RenderGraphContext context) {
            //todo: this is a lot of allocation/deallocation each frame
            //todo: look into renderGraphPool.tempArray, and maybe move this into the Run method?
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                passData.LightCount, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            for (int i = 0; i < passData.LightCount; i++) {
                packedLights[i] = new PackedLight(passData.Lights[i], 0);
            }
            
            context.cmd.SetBufferData(passData.LightingData.LightBuffer, packedLights, 0, 0, passData.LightCount);
            packedLights.Dispose();
            
            context.cmd.SetGlobalInteger(lightCountId, passData.LightCount);
            context.cmd.SetGlobalBuffer(lightBufferId, passData.LightingData.LightBuffer);
            context.cmd.SetGlobalBuffer(cullingResultsId, passData.LightingData.CullingResultsBuffer);
        }

        private void RenderShadows(SetupPassData passData, RenderGraphContext context) {
            context.cmd.SetGlobalTexture("DirectionalShadowAtlas", passData.LightingData.DirectionalShadowAtlas);
            //context.cmd.SetGlobalTexture(otherShadowAtlasId, passData.LightingData.OtherShadowAtlas);

            Matrix4x4[] directionalShadowMatrices =
                context.renderGraphPool.GetTempArray<Matrix4x4>(maxDirectionalShadows);
            Matrix4x4[] otherShadowMatrices = 
                context.renderGraphPool.GetTempArray<Matrix4x4>(maxOtherShadows);
            
            byte directionalShadows = 0;
            for (int i = 0; i < passData.LightCount; i++) {
                var vLight = passData.Lights[i];
                if (vLight.lightType == LightType.Directional &&
                    vLight.light.shadows != LightShadows.None &&
                    vLight.light.shadowStrength > 0 &&
                    directionalShadows < maxDirectionalShadows &&
                    //out bounds encapsulates visible shadow casters
                    cull.GetShadowCasterBounds(i, out _)
                ) {
                    var shadowSettings = new ShadowDrawingSettings(cull, i);
                    cull.ComputeDirectionalShadowMatricesAndCullingPrimitives( //todo: compute split count, index, and shadow Resolution
                        i, 0, 1, Vector3.zero, 512, 0f,
                        out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                        out ShadowSplitData splitData
                    );
                    shadowSettings.splitData = splitData;

                    directionalShadowMatrices[i] = ConvertToAtlasMatrix(
                        projectionMatrix * viewMatrix, 
                        Vector2.zero, 0
                    );
                    
                    context.cmd.SetRenderTarget(
                        passData.LightingData.DirectionalShadowAtlas, 
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.Store
                    );
                    context.cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                    /*context.renderContext.ExecuteCommandBuffer(context.cmd);
                    context.cmd.Clear();*/
                    
                    context.renderContext.DrawShadows(ref shadowSettings);
                    
                    directionalShadows++;
                }
            }
            
            context.cmd.SetViewProjectionMatrices(
                camera.worldToCameraMatrix,
                GL.GetGPUProjectionMatrix(camera.projectionMatrix, true)
            );
            context.cmd.SetGlobalTexture(directionalShadowAtlasId, passData.LightingData.DirectionalShadowAtlas);
            //context.cmd.SetGlobalTexture(otherShadowAtlasId, passData.LightingData.OtherShadowAtlas);

            context.cmd.SetGlobalMatrixArray(directionalShadowMatricesId, directionalShadowMatrices);
            context.cmd.SetGlobalMatrixArray(otherShadowAtlasId, otherShadowMatrices);
        }

        private void RenderLighting(SetupPassData passData, RenderGraphContext context) {
            context.cmd.SetComputeMatrixParam(shaderBundle.LightingShader, "unity_MatrixV", camera.worldToCameraMatrix);
            context.cmd.SetComputeMatrixParam(
                shaderBundle.LightingShader, "unity_MatrixInvVP", 
                (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse
            );
            context.cmd.SetComputeVectorParam(
                shaderBundle.LightingShader, "_WorldSpaceCameraPos", camera.transform.position
            );


            var tileCount = viewportParams.TileCount;
            context.cmd.DispatchCompute(
                shaderBundle.LightCullingShader, lightCullingKernelId,
                tileCount.x, tileCount.y, 1
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
            
            /*m.m00 = 0.5f * (m.m00 + m.m30);
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
            m.m23 = 0.5f * (m.m23 + m.m33);*/
            
            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }

        public override void Dispose() => viewportParamsBuffer.Release();
    }
}