using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class BlitPass {
        private static readonly Material litMaterial = CoreUtils.CreateEngineMaterial("Hidden/DumbLitPass");
        
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

        public static void Run(RenderGraph renderGraph, GBuffer gBuffer) {
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
            
            
            
            //CoreUtils.DrawFullScreen(context.cmd, passData.litMaterial, BuiltinRenderTextureType.CameraTarget);
            context.cmd.Blit(null, BuiltinRenderTextureType.CameraTarget, passData.litMaterial);
        }
    }
}