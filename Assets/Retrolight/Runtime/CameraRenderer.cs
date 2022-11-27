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

        private static readonly ShaderTagId gBufferPassId = new ShaderTagId("GBuffer");

        public CameraRenderer(RenderGraph renderGraph, uint pixelScale) {
            this.renderGraph = renderGraph;
            this.pixelScale = pixelScale;
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
            TextureDesc colorDesc = new TextureDesc(Screen.width, Screen.height) {
                colorFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, isSrgb),
                depthBufferBits = DepthBits.None,
                clearBuffer = true, //set back to true
                clearColor = Color.black,
                enableRandomWrite = false, 
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = name
            };
            return renderGraph.CreateTexture(colorDesc);
        }

        private TextureHandle CreateDepthTexture(string name) {
            TextureDesc depthDesc = new TextureDesc(Screen.width, Screen.height) {
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
        }

        private void BlitPass(GBuffer gBuffer) {
            using (var builder = renderGraph.AddRenderPass(
                "Blit Pass", 
                out BlitPassData passData,
                new ProfilingSampler("Blit Pass Profiler")
            )) {
                passData.gBuffer = gBuffer.ReadAll(builder);
                builder.SetRenderFunc<BlitPassData>(RenderBlitPass);
            }
        }

        private static void RenderBlitPass(BlitPassData passData, RenderGraphContext context) {
            //todo: setup gbuffer blit pass
            context.cmd.Blit(passData.gBuffer.depth, BuiltinRenderTextureType.CameraTarget);
        }
    }
}