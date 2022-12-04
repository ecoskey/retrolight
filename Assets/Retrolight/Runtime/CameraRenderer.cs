using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using RendererListDesc = UnityEngine.Rendering.RendererUtils.RendererListDesc;

namespace Retrolight.Runtime {
    public class CameraRenderer {
        private readonly RenderGraph renderGraph;
        private readonly uint pixelScale;

        private Camera camera;
        private CullingResults cull;

        private readonly Material litMaterial;
        private readonly int kernelId;
        private static readonly GlobalKeyword orthographicCamera = GlobalKeyword.Create("ORTHOGRAPHIC_CAMERA");

        private static readonly ShaderTagId gBufferPassId = new ShaderTagId("RetrolightGBuffer");

        public CameraRenderer(RenderGraph renderGraph, ComputeShader testShader, uint pixelScale) {
            this.renderGraph = renderGraph;
            this.pixelScale = pixelScale;
            this.litMaterial = CoreUtils.CreateEngineMaterial("Hidden/DumbLitPass");
            kernelId = testShader.FindKernel("CullLights");
        }
        
        public void Render(ScriptableRenderContext context, Camera camera) {
            this.camera = camera;

            ScriptableCullingParameters cullingParams;
            if (!camera.TryGetCullingParameters(out cullingParams)) return;
            cull = context.Cull(ref cullingParams);
            
            context.SetupCameraProperties(camera);
            
            var cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
            var renderGraphParams = new RenderGraphParameters {
                scriptableRenderContext = context, 
                commandBuffer = cmd, 
                currentFrameIndex = Time.frameCount,
            };

            using (renderGraph.RecordAndExecute(renderGraphParams)) {
                var gBuffer = GBufferPass();
                BlitPass(gBuffer);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            this.camera = null;
        }
        
        // ********************************************************************************************************** //
        
        private TextureHandle CreateColorTexture(string name) {
            bool isSrgb = QualitySettings.activeColorSpace == ColorSpace.Gamma;
            TextureDesc colorDesc = new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
                colorFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, isSrgb),
                depthBufferBits = DepthBits.None,
                clearBuffer = true,
                clearColor = Color.clear,
                enableRandomWrite = false, 
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = name
            };
            return renderGraph.CreateTexture(colorDesc);
        }

        private TextureHandle CreateDepthTexture(string name) {
            TextureDesc depthDesc = new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
                colorFormat = GraphicsFormat.None,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true, //set back to true
                clearColor = Color.black, //todo: is this correct?
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = name
            };
            return renderGraph.CreateTexture(depthDesc);
        }
        
        // ********************************************************************************************************** //

        class GBufferPassData {
            public GBuffer gBuffer;
            public RendererListHandle gBufferRendererList;
        }

        private GBuffer GBufferPass() {
            using (var builder = renderGraph.AddRenderPass(
                "Geometry Pass", 
                out GBufferPassData passData, 
                new ProfilingSampler("GBuffer Pass Profiler")
            )) {
                TextureHandle albedo = CreateColorTexture("Albedo");
                TextureHandle depth = CreateDepthTexture("Depth");
                TextureHandle normal = CreateColorTexture("Normal");
                TextureHandle attributes = CreateColorTexture("Attributes");
                //TextureHandle emission = CreateColorTexture("Emission");

                GBuffer gBuffer = new GBuffer(
                    builder.UseColorBuffer(albedo, 0), 
                    builder.UseDepthBuffer(depth, DepthAccess.Write), 
                    builder.UseColorBuffer(normal, 1),
                    builder.UseColorBuffer(attributes, 2)
                );
                passData.gBuffer = gBuffer;

                RendererListDesc gBufferRendererDesc  = new RendererListDesc(gBufferPassId, cull, camera) {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    renderQueueRange = RenderQueueRange.opaque
                };
                RendererListHandle gBufferRendererHandle = renderGraph.CreateRendererList(gBufferRendererDesc);
                passData.gBufferRendererList = builder.UseRendererList(gBufferRendererHandle);
                
                builder.SetRenderFunc<GBufferPassData>(RenderGBufferPass);

                return gBuffer;
            }
        }

        private static void RenderGBufferPass(GBufferPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.gBufferRendererList);
        }
        
        // ********************************************************************************************************** //

        class BlitPassData {
            public GBuffer gBuffer;
            public Material litMaterial;
            /*public TextureHandle testTex;
            public ComputeShader shader;
            public Camera camera;
            public GlobalKeyword orthoCameraKeyword;
            public int kernelIndex;
            public Vector2Int tilingSize;*/
        }

        private void BlitPass(GBuffer gBuffer) {
            using (var builder = renderGraph.AddRenderPass(
                "Blit Pass", 
                out BlitPassData passData,
                new ProfilingSampler("Blit Pass Profiler")
            )) {
                passData.gBuffer = gBuffer.ReadAll(builder);
                /*TextureDesc desc = new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
                    colorFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, false),
                    depthBufferBits = DepthBits.None,
                    clearBuffer = true,
                    clearColor = Color.clear,
                    enableRandomWrite = true, 
                    filterMode = FilterMode.Point,
                    msaaSamples = MSAASamples.None,
                    useDynamicScale = false,
                };*/
                /*passData.testTex = builder.CreateTransientTexture(desc);
                passData.shader = testShader;
                passData.camera = camera;
                passData.orthoCameraKeyword = orthographicCamera;
                passData.tilingSize = new Vector2Int(camera.pixelWidth / 8, camera.pixelHeight / 8);*/
                passData.litMaterial = litMaterial;
                builder.SetRenderFunc<BlitPassData>(RenderBlitPass);
            }
        }

        private static void RenderBlitPass(BlitPassData passData, RenderGraphContext context) {
            /*context.cmd.SetKeyword(passData.orthoCameraKeyword, passData.camera.orthographic);
            passData.shader.SetTexture(passData.kernelIndex, "ColorTex", passData.testTex);
            passData.shader.SetVector(
                "Resolution", 
                new Vector4(
                    passData.camera.pixelWidth, 
                    passData.camera.pixelHeight, 
                    1f / passData.camera.pixelWidth, 
                    1f / passData.camera.pixelHeight
                )
            );
            passData.shader.SetTexture(passData.kernelIndex, "Depth", passData.gBuffer.depth);
            context.cmd.DispatchCompute(
                passData.shader, passData.kernelIndex, 
                passData.tilingSize.x, passData.tilingSize.y, 1
            );
            context.cmd.Blit(passData.testTex, BuiltinRenderTextureType.CameraTarget);*/
            context.cmd.SetGlobalTexture("Albedo", passData.gBuffer.albedo);
            context.cmd.SetGlobalTexture("Depth", passData.gBuffer.depth);
            context.cmd.SetGlobalTexture("Normal", passData.gBuffer.normal);
            context.cmd.Blit(null, BuiltinRenderTextureType.CameraTarget, passData.litMaterial);
        }
    }
}