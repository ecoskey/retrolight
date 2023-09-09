using System;
using System.Runtime.InteropServices;
using Retrolight.Data;
using Retrolight.Data.Bundles;
using Retrolight.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using float4x4 = Unity.Mathematics.float4x4;
using half3 = Unity.Mathematics.half3;


namespace Retrolight.Passes {
    public class LightingPasses : RenderPass {
        private readonly ComputeShader lightCullingShader, lightingModel;
        private readonly LocalKeyword debugKeyword;
        private readonly int lightingKernelId, lightCullingKernelId;

        private static GlobalKeyword kEnableShadows = GlobalKeyword.Create("ENABLE_SHADOWS");
        private static GlobalKeyword kEnableSsao = GlobalKeyword.Create("ENABLE_SSAO");
        
        public LightingPasses(Retrolight retrolight) : this(retrolight, Option.None<ComputeShader>()) { } 
        
        public LightingPasses(
            Retrolight retrolight, 
            Option<ComputeShader> customLighting
        ) : base(retrolight) {
            // ReSharper disable once MergeConditionalExpression
            lightCullingShader = ShaderBundle.Instance.LightCullingShader;
            debugKeyword = new LocalKeyword(lightCullingShader, "EDITOR_DEBUG");
            lightingModel = customLighting.OrElse(ShaderBundle.Instance.LightingShader);
            lightingKernelId = lightingModel.FindKernel("Lighting");
            lightCullingKernelId = ShaderBundle.Instance.LightCullingShader.FindKernel("LightCulling");
        }
        

        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast)]
        private struct LightArrayJob : IJobParallelFor {
            [ReadOnly] public float4x4 viewMatrix;
            [ReadOnly] public NativeArray<VisibleLight> VisibleLights;
            public NativeArray<PackedLight> PackedLights;

            //intended to be used with NativeArray.Reinterpret on a VisibleLight
            /*[StructLayout(LayoutKind.Sequential)]
            public struct RawVisibleLight {
                public LightType type;
                public float4 color;
                public float4 _packing; //instead of screen space rectangle
                public float4x4 localToWorld;
                public float range;
                public float spotAngle;
                public int instanceID;
                private int _packing2; // instead of internal flags enum
            }*/

            private static PackedLight.Flags GetLightFlags(VisibleLight light) {
                var flags = PackedLight.Flags.None;
                /*if (light.light.shadows != LightShadows.None && light.light.shadowStrength > 0)
                    flags |= PackedLightFlags.Shadowed;*/
                return flags;
            }

            private static PackedLight.Type GetLightType(VisibleLight light) => light.lightType switch {
                LightType.Directional => PackedLight.Type.Directional,
                LightType.Point => PackedLight.Type.Point,
                LightType.Spot => PackedLight.Type.Spot,
                _ => PackedLight.Type.Spot //todo: support area lights in future
            };

            public void Execute(int index) {
                var vLight = VisibleLights[index];
                var lightTransformRaw = vLight.localToWorldMatrix;
                float4x4 lightTransform = UnsafeUtility.As<Matrix4x4, float4x4>(ref lightTransformRaw);
                float3 positionWS = lightTransform.c3.xyz;
                float4 dirWS = -lightTransform.c2;
                dirWS.w = 0; //multiply as direction, don't use offset
                var pLight = new PackedLight {
                    position = transform(viewMatrix, positionWS),
                    //position = positionWS,
                    type2_flags6 = (byte) ((byte) GetLightType(vLight) | (byte) GetLightFlags(vLight) << 2),
                    range = half(vLight.range),
                    //this.shadowIndex = shadowIndex;
                    color = half3(vLight.finalColor.AsVector().xyz),
                    cosAngle = half(cos(radians(vLight.spotAngle * 0.5f))),
                    //dir = half3(dirWS.xyz),
                    dir = half3(mul(viewMatrix, dirWS).xyz),
                    //shadowStrength = half(light.light.shadowStrength);
                };
                //pLight.position.z = -pLight.position.z;
                
                PackedLights[index] = pLight;
            }
        }

        private class LightAllocationPassData {
            public AllocatedLights AllocatedLights;
            public NativeArray<PackedLight> Lights;
            public JobHandle lightArrayJob;
            //public ShadowCastersCullingInfos CullingInfos;
        }

        public AllocatedLights AllocateLights() {
            using var builder = AddRenderPass(
                "Light Allocation Pass", RenderLightAllocation, 
                out LightAllocationPassData passData
            );
            
            //necessary to not run the light array job without completing it
            builder.AllowPassCulling(false);
            //builder.AllowGlobalStateModification(true); //todo: necessary?
            
            var lightsBufferDesc = new BufferDesc(Constants.MaximumLights, UnsafeUtility.SizeOf<PackedLight>()) {
                name = Constants.LightBufferName,
                target = GraphicsBuffer.Target.Structured
            };
            //var lightBuffer = CreateUseBuffer(builder, lightsDesc, AccessFlags.Write);
            var lightBuffer = builder.WriteBuffer(renderGraph.CreateBuffer(lightsBufferDesc));

            //var rawVisibleLights = cull.visibleLights.Reinterpret<LightArrayJob.RawVisibleLight>();
            //visibleLights.Dispose();
            
            int lightCount = Math.Min(cull.visibleLights.Length, Constants.MaximumLights);
            NativeArray<PackedLight> packedLights = new NativeArray<PackedLight>(
                lightCount, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory
            );
            passData.Lights = packedLights;

            //float4x4 viewMatrix = mul(float4x4.Scale(1, 1, -1), camera.worldToCameraMatrix);
            float4x4 viewMatrix = camera.worldToCameraMatrix;
            //viewMatrix = mul(float4x4.Scale(1, 1, -1), viewMatrix);
            //Unity BS to invert Z direction for view matrix, equivalent to above matrix calculation
            //if (!camera.IsSceneView()) {
            /*viewMatrix.c0.z = -viewMatrix.c0.z;
            viewMatrix.c1.z = -viewMatrix.c1.z;
            viewMatrix.c2.z = -viewMatrix.c2.z;
            viewMatrix.c3.z = -viewMatrix.c3.z;*/
            //}
            
            var lightArrayJob = new LightArrayJob {
                VisibleLights = cull.visibleLights,
                PackedLights = packedLights,
                viewMatrix = viewMatrix,
            };
            
            passData.lightArrayJob = lightArrayJob.Schedule(lightCount, 32);

            /*
            byte directionalShadowSplits = 0;
            byte otherShadowSplits = 0;
            //visible light indices, not shadow atlas indices
            List<ShadowedLight> shadowedDirectionalLights = new List<ShadowedLight>();

            byte directionalCascades = camera.orthographic ? (byte) 1 : shadowSettings.directionalCascades;
            int maxDirectionalSplits = directionalCascades * shadowSettings.maxDirectionalShadows;
            int maxOtherSplits = shadowSettings.maxOtherShadows;*/

            /*
            NativeArray<ShadowSplitData> shadowSplits = new NativeArray<ShadowSplitData>(
                maxDirectionalSplits + maxOtherSplits, 
                Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            
            NativeArray<LightShadowCasterCullingInfo> lightShadowCasterCullingInfos = 
                new NativeArray<LightShadowCasterCullingInfo>(
                    lightCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
                );
                */
            
            
            
            /*for (int i = 0; i < lightCount; i++) {
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
                        /*case LightType.Spot: break;
                        case LightType.Point: break;#1#
                        default: break;
                    }
                }
                packedLights[i] = new PackedLight(vLight, shadowSplitIndex);
            }*/

            /*passData.CullingInfos = new ShadowCastersCullingInfos {
                splitBuffer = shadowSplits,
                perLightInfos = lightShadowCasterCullingInfos
            };*/
            
            var allocatedLights = new AllocatedLights(lightCount, lightBuffer/*, shadowedDirectionalLights*/);
            passData.AllocatedLights = allocatedLights;
            
            return allocatedLights;
        }
        
        private void RenderLightAllocation(LightAllocationPassData passData, RenderGraphContext ctx) {
            var lightCount = passData.AllocatedLights.LightCount;
            ctx.cmd.SetGlobalInteger(Constants.LightCountId, lightCount);

            /*for (int i = 0; i < lightCount; i++) {
                Debug.Log(passData.Lights[i].position.ToString());
            }*/

            passData.lightArrayJob.Complete();
            ctx.cmd.SetBufferData(
                passData.AllocatedLights.LightsBuffer, passData.Lights, 
                0, 0, lightCount
            );
            //passData.tempVLights.Dispose();
            passData.Lights.Dispose();
            ctx.cmd.SetGlobalBuffer(Constants.LightBufferId, passData.AllocatedLights.LightsBuffer);
            
            //ctx.renderContext.CullShadowCasters(cull, passData.CullingInfos); //evil
        }
        
        /*private class ShadowsPassData {
            public AllocatedLights AllocatedLights;
            public ShadowData ShadowData;
            public int DirectionalAtlasSplit;
            public ShadowmapSize DirectionalSplitSize;
            public RendererListHandle[] DirectionalShadowRenderers;
            //public RendererListHandle[] OtherShadowRenderers;
        }*/
        
        /*public Option<ShadowData> RunShadows(AllocatedLights allocatedLights) {
            //todo: make actual check for invalid ShadowData in struct, god I wish we had ADTs
            if (allocatedLights.ShadowedDirectionalLights.Count <= 0) return Option.None<ShadowData>(); 
            using var builder = AddRenderPass("Shadows Pass", RenderShadows, out ShadowsPassData passData);

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
            );#1#

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
            
            var shadowData = ShadowData.Create(
                //todo: again check if this allows for depth testing
                builder.ReadWriteTexture(renderGraph.CreateTexture(directionalAtlasDesc)),
                directionalShadowMatrices
            );
            passData.ShadowData = shadowData;

            return Option.Some(shadowData);
        }
        */

        /*private void RenderShadows(ShadowsPassData passData, RenderGraphContext ctx) {
            //todo: check if this allows for depth testing
            CoreUtils.SetRenderTarget(ctx.cmd, passData.ShadowData.DirectionalShadowAtlas, ClearFlag.Depth);

            int numValid = passData.DirectionalShadowRenderers.Count(l => l.IsValid());
            int numShadows = passData.DirectionalShadowRenderers.Length;
            //Debug.Log(string.Format("{0} of {1} shadow rendererLists are valid", numValid.ToString(), numShadows.ToString()));
            
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
        */
        
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
        
        private class LightCullingPassData {
            //public AllocatedLights AllocatedLights;
            public BufferHandle CullingResultsBuffer;
            #if UNITY_EDITOR
            public Option<TextureHandle> CullingDebugTex;
            #endif
        }

        public BufferHandle CullLights(TextureHandle depthTex, AllocatedLights allocatedLights, bool debug = false) {
            using var builder = AddRenderPass(
                "Light Culling Pass", 
                RenderLightCulling, out LightCullingPassData passData);
            
            //builder.EnableAsyncCompute(true);
            //builder.AllowGlobalStateModification(true);
            //builder.UseTexture(gbuffer.Depth, AccessFlags.Read);
            allocatedLights.ReadAll(builder);
            builder.ReadTexture(depthTex);
            
            //passData.AllocatedLights = allocatedLights.UseAll(builder, AccessFlags.Read);
            #if UNITY_EDITOR
            if (debug) {
                var debugDesc = TextureUtils.ColorTex(Vector2.one, "Culling Debug Tex");
                debugDesc.enableRandomWrite = true;
                passData.CullingDebugTex = Option.Some(builder.CreateTransientTexture(debugDesc));
            } else {
                passData.CullingDebugTex = Option.None<TextureHandle>();
            }
            #endif

            var cullingResultsSize = 
                MathUtils.NextMultipleOf(Constants.MaximumLights, Constants.UIntBitSize)
                * viewportParams.TileCount.x
                * viewportParams.TileCount.y;
            
            var cullingResultsDesc = new BufferDesc(cullingResultsSize, sizeof(uint)) {
                name = Constants.LightCullingResultsBufferName,
                target = GraphicsBuffer.Target.Raw
            };

            passData.CullingResultsBuffer = builder.WriteBuffer(renderGraph.CreateBuffer(cullingResultsDesc));
            //builder.UseBuffer(builder, cullingResultsDesc, AccessFlags.Write);
            return passData.CullingResultsBuffer;
        }

        private void RenderLightCulling(LightCullingPassData passData, RenderGraphContext ctx) {
            var groups = viewportParams.TileCount;
            ctx.cmd.SetComputeBufferParam(
                lightCullingShader, lightCullingKernelId, 
                Constants.LightCullingResultsId, passData.CullingResultsBuffer
            );

            #if UNITY_EDITOR
            bool debugEnabled = passData.CullingDebugTex.Enabled;
            ctx.cmd.SetKeyword(lightCullingShader, debugKeyword, debugEnabled);
            if (debugEnabled) {
                ctx.cmd.SetComputeTextureParam(
                    lightCullingShader, lightCullingKernelId, 
                    "CullingDebugTex", passData.CullingDebugTex.Value
                );
            }
            #endif
            
            ctx.cmd.SetComputeMatrixParam(
                lightCullingShader, "unity_MatrixInvP",
             Matrix4x4.Scale(new Vector3(1, 1, -1)) * GL.GetGPUProjectionMatrix(camera.projectionMatrix, true).inverse
            );

            ctx.cmd.DispatchCompute(
                lightCullingShader, lightCullingKernelId, 
                groups.x, groups.y, 1
            );

            ctx.cmd.SetGlobalBuffer(Constants.LightCullingResultsId, passData.CullingResultsBuffer);
        }
        
        
        //LIGHTING
        //--------------------------------------------------------------------------------------------------------------

        private class LightingPassData {
            public TextureHandle FinalColorTex;
            public BufferHandle hilbertIndices;
            public Option<ShadowData> ShadowData;
            public Option<TextureHandle> Ssao;
        }
        
        public TextureHandle RunLighting(
            GBuffer gBuffer, TextureHandle depthTex, 
            AllocatedLights allocatedLights, BufferHandle lightCullingResults//,
            //BufferHandle hilbertIndices
        ) => RunLighting(
            gBuffer, depthTex, allocatedLights, lightCullingResults, 
            /*hilbertIndices, */Option.None<ShadowData>(), Option.None<TextureHandle>()
        );
        
        public TextureHandle RunLighting(
            GBuffer gBuffer, TextureHandle depthTex, 
            AllocatedLights allocatedLights, BufferHandle lightCullingResults,
            /*BufferHandle hilbertIndices, */ShadowData shadows, TextureHandle ssao
        ) => RunLighting(
            gBuffer, depthTex, allocatedLights, lightCullingResults, 
            /*hilbertIndices, */Option.Some(shadows), Option.Some(ssao)
        );

        public TextureHandle RunLighting(
            GBuffer gBuffer, TextureHandle depthTex, AllocatedLights allocatedLights, 
            BufferHandle lightCullingResults, //BufferHandle hilbertIndices,
            Option<ShadowData> shadows, Option<TextureHandle> ssao
        ) {
            using var builder = AddRenderPass("Lighting Pass", RenderLighting, out LightingPassData passData);
            
            builder.ReadTexture(depthTex);
            gBuffer.ReadAll(builder);
            allocatedLights.ReadAll(builder);
            builder.ReadBuffer(lightCullingResults);

            var finalColorDesc = TextureUtils.ColorTex(
                Vector2.one, TextureUtils.PreferHdrFormat(useHDR),
                Constants.FinalColorTexName
            );
            finalColorDesc.enableRandomWrite = true;
            passData.FinalColorTex = builder.WriteTexture(renderGraph.CreateTexture(finalColorDesc));//CreateUseTex(builder, finalColorDesc);

            //passData.hilbertIndices = hilbertIndices;
            passData.ShadowData = shadows.Map(s => s.ReadAll(builder));
            passData.Ssao = ssao.Map(t => builder.ReadTexture(t));
            
            return passData.FinalColorTex;
        }

        private void RenderLighting(LightingPassData passData, RenderGraphContext ctx) {
            var skyboxRenderer = ctx.renderContext.CreateSkyboxRendererList(camera);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, skyboxRenderer);
            var groups = viewportParams.TileCount;

            //ctx.cmd.SetKeyword(kEnableShadows, passData.ShadowData.Enabled);
            //ctx.cmd.SetKeyword(kEnableSsao, passData.Ssao.Enabled);
            
            //todo: set global from shadows pass
            //ctx.cmd.SetComputeTextureParam(lightingModel, lightingKernelId, "DirectionalShadowAtlas", passData.ShadowData.DirectionalShadowAtlas);
            //ctx.cmd.SetComputeMatrixArrayParam(lightingModel, "DirectionalShadowMatrices", passData.ShadowData.DirectionalShadowMatrices);
            
            //ctx.cmd.SetComputeBufferParam(lightingModel, lightingKernelId, "HilbertIndices", passData.hilbertIndices);
            
            //ctx.cmd.SetComputeMatrixParam(lightingModel, "unity_MatrixV", camera.worldToCameraMatrix);

            //Matrix4x4 invViewMatrix = camera.cameraToWorldMatrix;
            
            
            ctx.cmd.SetComputeMatrixParam(
                lightingModel, "unity_MatrixInvP", 
                Matrix4x4.Scale(new Vector3(1, 1, -1)) * GL.GetGPUProjectionMatrix(camera.projectionMatrix, true).inverse
            );
            
            /*ctx.cmd.SetComputeMatrixParam(
                lightingModel, "unity_MatrixInvVP", 
                invViewMatrix * GL.GetGPUProjectionMatrix(camera.projectionMatrix, true).inverse
            );*/
            
            
            
            //ctx.cmd.SetComputeVectorParam(lightingModel, "_WorldSpaceCameraPos", camera.transform.position);
            
            ctx.cmd.SetComputeTextureParam(
                lightingModel, lightingKernelId, 
                Constants.FinalColorTexId, passData.FinalColorTex
            );
            
            ctx.cmd.DispatchCompute(lightingModel, lightingKernelId, groups.x, groups.y, 1);
        }
    }
}