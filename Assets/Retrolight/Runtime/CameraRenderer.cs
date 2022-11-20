using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    public class CameraRenderer {
        private RenderGraph renderGraph;

        public CameraRenderer(RenderGraph renderGraph) {
            this.renderGraph = renderGraph;
        }
        
        public void Render(ScriptableRenderContext context, Camera camera) {
            var cmd = new CommandBuffer();
            var renderGraphParams = new RenderGraphParameters {
                scriptableRenderContext = context, 
                commandBuffer = cmd, 
                currentFrameIndex = Time.frameCount,
            };

            using (renderGraph.RecordAndExecute(renderGraphParams)) {
                TextureHandle opaqueTexture = renderGraph.CreateTexture(new TextureDesc(Vector2.one) {
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    clearBuffer = true,
                    clearColor = Color.black,
                    name = "Opaque Texture"
                });
                
                SetupGeometryPass(opaqueTexture);
            }
        }

        class GeometryPassData {
            public TextureHandle opaqueTexture;
            //public TextureHandle depthNormalsTexture;
        }

        private void SetupGeometryPass(TextureHandle opaqueTexture) {
            using (var builder = renderGraph.AddRenderPass("Geometry Pass", out GeometryPassData passData)) {
                

                /*TextureHandle depthNormalsTexture = renderGraph.CreateTexture(new TextureDesc(Vector2.one) {
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    clearBuffer = true,
                    clearColor = Color.black,
                    name = "DepthNormals Texture"
                });*/

                passData.opaqueTexture = builder.WriteTexture(opaqueTexture);
                //passData.depthNormalsTexture = builder.WriteTexture(depthNormalsTexture);

                builder.SetRenderFunc<GeometryPassData>(RenderGeometryPass);
            }
        }

        private void RenderGeometryPass(GeometryPassData passData, RenderGraphContext context) { 
            
        }

        class BlitPassData {
            public TextureHandle opaqueTexture;
            public TextureHandle colorTarget;
        }

        private void SetupBlitPass(TextureHandle _opaqueTexture) {
            using (var builder = renderGraph.AddRenderPass("Geometry Pass", out GeometryPassData passData)) {
                

                /*TextureHandle depthNormalsTexture = renderGraph.CreateTexture(new TextureDesc(Vector2.one) {
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    clearBuffer = true,
                    clearColor = Color.black,
                    name = "DepthNormals Texture"
                });*/
                TextureHandle opaqueTexture = builder.ReadTexture(_opaqueTexture);

                builder.UseColorBuffer()
                //passData.depthNormalsTexture = builder.WriteTexture(depthNormalsTexture);

                builder.SetRenderFunc<GeometryPassData>(RenderGeometryPass);
            }
        }

        private void RenderBlitPass(BlitPassData passData, RenderGraphContext context) {
            context.cmd.Blit(passData.opaqueTexture, passData.colorTarget);
        }
    }
}