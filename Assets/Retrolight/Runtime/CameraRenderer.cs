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

        private static readonly ShaderTagId geometryPassId = new ShaderTagId("Geometry");

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
                GeometryPassData geometryPassData = GeometryPass();
                BlitPass(geometryPassData.albedo);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            this.camera = null;
        }

        class GeometryPassData {
            public TextureHandle albedo;
            public TextureHandle depth;

            public RendererListHandle geometryRenderList;
        }

        private GeometryPassData GeometryPass() {
            using (var builder = renderGraph.AddRenderPass("Geometry Pass", out GeometryPassData passData)) {
                TextureHandle opaque = renderGraph.CreateTexture(new TextureDesc(Vector2.one) {
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    clearBuffer = true,
                    clearColor = Color.clear,
                    name = "Opaque Texture"
                });
                passData.albedo = builder.UseColorBuffer(opaque, 0);
                
                RendererListDesc geometryRenderDesc  = new RendererListDesc(geometryPassId, cull, camera) {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    renderQueueRange = RenderQueueRange.opaque,
                };
                RendererListHandle geometryRenderHandle = renderGraph.CreateRendererList(geometryRenderDesc);
                passData.geometryRenderList = builder.UseRendererList(geometryRenderHandle);
        
                
                builder.SetRenderFunc<GeometryPassData>(RenderGeometryPass);
                return passData;
            }
        }

        private void RenderGeometryPass(GeometryPassData passData, RenderGraphContext context) {
            CoreUtils.DrawRendererList(context.renderContext, context.cmd, passData.geometryRenderList);
        }
        
        

        class BlitPassData {
            public TextureHandle albedo;
        }

        private void BlitPass(TextureHandle albedo) {
            using (var builder = renderGraph.AddRenderPass("Geometry Pass", out BlitPassData passData)) {
                passData.albedo = builder.ReadTexture(albedo);
                builder.SetRenderFunc<BlitPassData>(RenderBlitPass);
            }
        }

        private void RenderBlitPass(BlitPassData passData, RenderGraphContext context) {
            //context.cmd.Blit(passData.albedo, BuiltinRenderTextureType.CameraTarget);
            if (camera.clearFlags == CameraClearFlags.Skybox) { context.renderContext.DrawSkybox(camera); }
            
        }
    }
}