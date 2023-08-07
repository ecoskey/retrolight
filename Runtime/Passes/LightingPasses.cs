using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using Data;
using Unity.Collections;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using Util;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;


namespace Passes {
    public class LightingPasses : RenderPass {
        private readonly ComputeShader lightingModel;
        private readonly int lightingKernelId, lightCullingKernelId;
        
        public LightingPasses(Retrolight retrolight, ComputeShader customLighting = null) : base(retrolight) {
            // ReSharper disable once MergeConditionalExpression
            lightingModel = customLighting is null ? ShaderBundle.Instance.LightingShader : customLighting;
            lightingKernelId = lightingModel.FindKernel("Lighting");
            lightCullingKernelId = ShaderBundle.Instance.LightCullingShader.FindKernel("LightCulling");
        }
        
        private class LightAllocationPassData {
            public AllocatedLights AllocatedLights;
            public NativeArray<PackedLight> Lights;
            public ShadowCastersCullingInfos CullingInfos;
        }

        private class LightCullingPassData {
            public AllocatedLights AllocatedLights;
            public BufferHandle CullingResultsBuffer;
        }

        private class LightingPassData {
            public TextureHandle FinalColorTex;
            public ShadowData ShadowData;
        }

        private class ShadowsPassData {
            public AllocatedLights AllocatedLights;
            public ShadowData ShadowData;
            public int DirectionalAtlasSplit;
            public ShadowmapSize DirectionalSplitSize;
            public RendererListHandle[] DirectionalShadowRenderers;
            //public RendererListHandle[] OtherShadowRenderers;
        }
        
        
        public AllocatedLights AllocateLights() {
            using var builder = AddRenderPass<LightAllocationPassData>(
                "Light Allocation Pass", out var passData, RenderLightAllocation
            );
            
            //builder.AllowGlobalStateModification(true); //todo: necessary?
            
            var lightsBufferDesc = new BufferDesc(Constants.MaximumLights, Marshal.SizeOf<PackedLight>()) {
                name = Constants.LightBufferName,
                target = GraphicsBuffer.Target.Structured
            };
            //var lightBuffer = CreateUseBuffer(builder, lightsDesc, AccessFlags.Write);
            var lightBuffer = builder.WriteBuffer(renderGraph.CreateBuffer(lightsBufferDesc));
            
            int lightCount = Math.Min(cull.visibleLights.Length, Constants.MaximumLights);
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                lightCount, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );
            passData.Lights = packedLights;

            byte directionalShadowSplits = 0;
            byte otherShadowSplits = 0;
            //visible light indices, not shadow atlas indices
            List<ShadowedLight> shadowedDirectionalLights = new List<ShadowedLight>();
            
            //List<int> shadowedOtherLights = new List<int>();
            
            //todo: persist over multiple frames to avoid allocation?
            
            

            //List<ShadowSplitData> shadowSplits = new List<ShadowSplitData>();
            //List<LightShadowCasterCullingInfo> lightShadowCasterCullingInfos = new List<LightShadowCasterCullingInfo>();
            
            

            byte directionalCascades = camera.orthographic ? (byte) 1 : shadowSettings.directionalCascades;
            int maxDirectionalSplits = directionalCascades * shadowSettings.maxDirectionalShadows;
            int maxOtherSplits = shadowSettings.maxOtherShadows;

            NativeArray<ShadowSplitData> shadowSplits = new NativeArray<ShadowSplitData>(
                maxDirectionalSplits + maxOtherSplits, 
                Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            
            NativeArray<LightShadowCasterCullingInfo> lightShadowCasterCullingInfos = 
                new NativeArray<LightShadowCasterCullingInfo>(
                    lightCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
                );
            

            for (int i = 0; i < lightCount; i++) {
                var vLight = cull.visibleLights[i];
                var light = vLight.light;
                byte shadowSplitIndex = 0; //for setting a PackedLight's reference to the shadow matrix
                
                lightShadowCasterCullingInfos[i] = new LightShadowCasterCullingInfo {
                    splitRange = new RangeInt(0, 0),
                    projectionType = BatchCullingProjectionType.Unknown
                };
                
                if (
                    light.shadows != LightShadows.None
                    && light.shadowStrength > 0f
                    && cull.GetShadowCasterBounds(i, out _)
                ) {
                    switch (light.type) {
                        case LightType.Directional:
                            if (directionalShadowSplits + directionalCascades > maxDirectionalSplits) break;
                            shadowSplitIndex = directionalShadowSplits;
                            directionalShadowSplits += directionalCascades;

                            var splitIndex = directionalShadowSplits + otherShadowSplits;
                            directionalShadowSplits++;

                            lightShadowCasterCullingInfos[i] = new LightShadowCasterCullingInfo {
                                splitRange = new RangeInt(splitIndex, directionalCascades),
                                projectionType = BatchCullingProjectionType.Orthographic
                            };
                                
                            for (byte j = 0; j < directionalCascades; j++) {
                                cull.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                                    i, j, directionalCascades, new Vector3(0.1f, 0.25f, 0.5f), //todo: configure ratios
                                    (int) shadowSettings.directionalShadowmapSize, 0f,
                                    out Matrix4x4 matrixV, out Matrix4x4 matrixP,
                                    out ShadowSplitData shadowSplitData
                                );
                                shadowSplits[directionalShadowSplits++ + otherShadowSplits] = shadowSplitData;

                                shadowedDirectionalLights.Add(new ShadowedLight(i, matrixV, matrixP));
                            }
                            break;
                        case LightType.Spot: break;
                        case LightType.Point: break;
                    }
                }
                packedLights[i] = new PackedLight(vLight, shadowSplitIndex);
            }

            passData.CullingInfos = new ShadowCastersCullingInfos {
                splitBuffer = shadowSplits,
                perLightInfos = lightShadowCasterCullingInfos
            };
            
            var allocatedLights = new AllocatedLights(lightCount, lightBuffer, shadowedDirectionalLights);
            passData.AllocatedLights = allocatedLights;
            
            return allocatedLights;
        }
        
        private void RenderLightAllocation(LightAllocationPassData passData, RenderGraphContext ctx) {
            var lightCount = passData.AllocatedLights.LightCount;
            ctx.cmd.SetGlobalInteger(Constants.LightCountId, lightCount);

            ctx.cmd.SetBufferData(
                passData.AllocatedLights.LightsBuffer, passData.Lights, 
                0, 0, lightCount
            );
            passData.Lights.Dispose();
            ctx.cmd.SetGlobalBuffer(Constants.LightBufferId, passData.AllocatedLights.LightsBuffer);
            
            ctx.renderContext.CullShadowCasters(cull, passData.CullingInfos);
        }
        
        public ShadowData RunShadows(AllocatedLights allocatedLights) {
            //if (shadowedLightRenderCount <= 0)) return;
            using var builder = AddRenderPass<ShadowsPassData>("Shadows Pass", out var passData, RenderShadows);

            passData.AllocatedLights = allocatedLights;
            
            int directionalCascades = camera.orthographic ? 1 : shadowSettings.directionalCascades;
            GetAtlasSize(
                shadowSettings.directionalShadowmapSize, 
                allocatedLights.ShadowedDirectionalLights.Count * directionalCascades, 
                out var directionalAtlasSize, out var directionalAtlasSplit
            );

            passData.DirectionalAtlasSplit = directionalAtlasSplit;
            /*GetAtlasSize(
                shadowSettings.otherShadowmapSize, _otherSplits, 
                out var otherAtlasSize, out var otherAtlasSplit
            );*/

            var directionalAtlasDesc = GetAtlasDesc(directionalAtlasSize);
            //TextureDesc otherAtlasDesc = GetAtlasDesc(otherAtlasSize);
            
            //render each shadow atlas
            //todo: mutating this bad or good?
            

            RendererListHandle[] directionalShadowRenderers =
                new RendererListHandle[allocatedLights.ShadowedDirectionalLights.Count];
            passData.DirectionalShadowRenderers = directionalShadowRenderers;
            Matrix4x4[] directionalShadowMatrices = new Matrix4x4[allocatedLights.ShadowedDirectionalLights.Count];
            for (int i = 0; i < passData.AllocatedLights.ShadowedDirectionalLights.Count; i++) {
                var shadowedDirLight = passData.AllocatedLights.ShadowedDirectionalLights[i];
                var shadowDrawingSettings = new ShadowDrawingSettings(cull, shadowedDirLight.VisibleLightIndex);
                var shadowRenderer = renderGraph.CreateShadowRendererList(ref shadowDrawingSettings);
                //todo: optimize? it seems like renderer lists are duplicated across cascades
                builder.UseRendererList(shadowRenderer);
                directionalShadowRenderers[i] = directionalShadowRenderers[i];

                Rect atlasViewport = GetTileViewport(
                    i, directionalAtlasSplit, 
                    (int) shadowSettings.directionalShadowmapSize
                );
                
                directionalShadowMatrices[i] = ConvertToAtlasMatrix(
                    shadowedDirLight.MatrixP * shadowedDirLight.MatrixV,
                    atlasViewport.min, directionalAtlasSplit //todo: check that viewport.min is correct
                );
            }
            
            var shadowData = new ShadowData(
                //todo: again check if this allows for depth testing
                builder.ReadWriteTexture(renderGraph.CreateTexture(directionalAtlasDesc)),
                directionalShadowMatrices
            );
            passData.ShadowData = shadowData;

            return shadowData;
        }
        
        

        private void RenderShadows(ShadowsPassData passData, RenderGraphContext ctx) {
            //todo: check if this allows for depth testing
            CoreUtils.SetRenderTarget(ctx.cmd, passData.ShadowData.DirectionalShadowAtlas, ClearFlag.Depth);

            int numValid = passData.DirectionalShadowRenderers.Count(l => l.IsValid());
            int numShadows = passData.DirectionalShadowRenderers.Length;
            Debug.Log($"{numValid} of {numShadows} shadow rendererLists are valid");
            
            for (int i = 0; i < passData.DirectionalShadowRenderers.Length; i++) {
                ctx.cmd.SetViewport(
                    GetTileViewport(i, passData.DirectionalAtlasSplit, (int) passData.DirectionalSplitSize)
                );
                var shadowedDirLight = passData.AllocatedLights.ShadowedDirectionalLights[i];
                ctx.cmd.SetViewProjectionMatrices(shadowedDirLight.MatrixV, shadowedDirLight.MatrixP);
                var rendererList = passData.DirectionalShadowRenderers[i];

                CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, rendererList);
            }
            
            //todo: redo above section for other shadow types
            
            ctx.cmd.SetGlobalMatrixArray("DirectionalShadowMatrices", passData.ShadowData.DirectionalShadowMatrices);
            ctx.cmd.SetGlobalTexture("DirectionalShadowAtlas", passData.ShadowData.DirectionalShadowAtlas);
            
            //todo: just save/recalculate view and projection matrices, b/c that's the only thing being invalidated
            ctx.cmd.SetViewProjectionMatrices(
                camera.worldToCameraMatrix, 
                GL.GetGPUProjectionMatrix(camera.projectionMatrix, true)
            );
            //ctx.cmd.SetupCameraProperties(camera);
        }
        
        private static void GetAtlasSize(
            ShadowmapSize splitSize, int splits, 
            out ShadowmapSize atlasSize, out int atlasSplit
        ) {
            atlasSplit = splits switch {
                1 => 1,
                <= 4 => 2,
                <= 16 => 4,
                <= 64 => 8,
                _ => throw new ArgumentException($"Invalid shadow split count: {splits}")
            };
            var rawAtlasSize = atlasSplit * (int) splitSize;
            if (rawAtlasSize > 4096) throw new ArgumentException($"Shadow split size {splitSize} is too large");
            atlasSize = (ShadowmapSize) rawAtlasSize;
        }
        
        private static TextureDesc GetAtlasDesc(ShadowmapSize size) {
            int rawSize = (int) size;
            return new TextureDesc(rawSize, rawSize) { //todo: set size of atlas from settings
                colorFormat = TextureUtils.ShadowMapFormat,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true,
                clearColor = Color.clear,
                enableRandomWrite = false,
                filterMode = FilterMode.Bilinear,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = "Directional Shadow Atlas"
            };
        }
        
        private static Rect GetTileViewport (int index, int split, float tileSize) {
            Vector2 offset = new Vector2(index % split, index / split);
            return new Rect(
                offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
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

        public BufferHandle CullLights(GBuffer gbuffer, AllocatedLights allocatedLights) {
            using var builder = AddRenderPass<LightCullingPassData>(
                "Light Culling Pass", out var passData, 
                RenderLightCulling
            );
            
            //builder.EnableAsyncCompute(true);
            //builder.AllowGlobalStateModification(true);
            //builder.UseTexture(gbuffer.Depth, AccessFlags.Read);
            builder.ReadTexture(gbuffer.Depth);
            
            //passData.AllocatedLights = allocatedLights.UseAll(builder, AccessFlags.Read);
            
            var cullingResultsDesc = new BufferDesc(
                MathUtils.NextMultipleOf(Constants.MaximumLights, Constants.UIntBitSize) * 
                viewportParams.TileCount.x * viewportParams.TileCount.y,
                sizeof(uint)
            ) {
                name = Constants.LightCullingResultsBufferName,
                target = GraphicsBuffer.Target.Raw
            };

            passData.CullingResultsBuffer = builder.WriteBuffer(renderGraph.CreateBuffer(cullingResultsDesc));
            //builder.UseBuffer(builder, cullingResultsDesc, AccessFlags.Write);
            return passData.CullingResultsBuffer;
        }

        private void RenderLightCulling(LightCullingPassData passData, RenderGraphContext ctx) {
            var lightCullingShader = ShaderBundle.Instance.LightCullingShader;
            
            ctx.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, 
                Constants.LightCullingResultsId, passData.CullingResultsBuffer
            );
            
            RenderUtils.DispatchCompute(
                ctx.cmd, lightCullingShader, lightCullingKernelId, 
                int3(viewportParams.TileCount.xy, 1)
            );

            ctx.cmd.SetGlobalBuffer(Constants.LightCullingResultsId, passData.CullingResultsBuffer);
        }

        public TextureHandle RunLighting(
            GBuffer gBuffer, AllocatedLights allocatedLights, 
            BufferHandle lightCullingResults, ShadowData shadows
        ) {
            using var builder = AddRenderPass<LightingPassData>("Lighting Pass", out var passData, RenderLighting);

            passData.ShadowData = shadows.ReadAll(builder);//shadows.UseAll(builder, AccessFlags.Read);
            gBuffer.ReadAll(builder);//gBuffer.UseAll(builder, AccessFlags.Read);
            allocatedLights.ReadAll(builder);//allocatedLights.UseAll(builder);
            builder.ReadBuffer(lightCullingResults);//builder.UseBuffer(lightCullingResults, AccessFlags.Read);

            var finalColorDesc = TextureUtils.ColorTex(
                float2(1), TextureUtils.PreferHdrFormat(useHDR),
                Constants.FinalColorTexName
            );
            finalColorDesc.enableRandomWrite = true;

            passData.FinalColorTex = builder.WriteTexture(renderGraph.CreateTexture(finalColorDesc));//CreateUseTex(builder, finalColorDesc);
            return passData.FinalColorTex;
        }
        
        private void RenderLighting(LightingPassData passData, RenderGraphContext ctx) {
            //var skyboxRenderer = ctx.renderContext.CreateSkyboxRendererList(camera);
            //CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, skyboxRenderer);
            
            var groups = int3(viewportParams.TileCount.xy, 1);
            //todo: set global from shadows pass
            //ctx.cmd.SetComputeTextureParam(lightingModel, lightingKernelId, "DirectionalShadowAtlas", passData.ShadowData.DirectionalShadowAtlas);
            ctx.cmd.SetComputeMatrixArrayParam(lightingModel, "DirectionalShadowMatrices", passData.ShadowData.DirectionalShadowMatrices);
            
            ctx.cmd.SetComputeMatrixParam(lightingModel, "unity_MatrixV", camera.worldToCameraMatrix);
            ctx.cmd.SetComputeMatrixParam(
                lightingModel, "unity_MatrixInvVP", 
                (GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix).inverse
            );
            ctx.cmd.SetComputeVectorParam(
                lightingModel, "_WorldSpaceCameraPos", camera.transform.position
            );
            
            ctx.cmd.SetComputeTextureParam(
                lightingModel, lightingKernelId, 
                Constants.FinalColorTexId, passData.FinalColorTex
            );
            
            RenderUtils.DispatchCompute(ctx.cmd, lightingModel, lightingKernelId, groups);
        }
    }
}