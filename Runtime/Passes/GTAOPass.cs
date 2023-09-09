using System.Runtime.InteropServices;
using Retrolight.Data;
using Retrolight.Data.Bundles;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Passes {
    public class GTAOPass : RenderPass {
        private static readonly TextureDesc filteredDepthDesc;
        private static readonly TextureDesc edgesTexDesc; 
        private static readonly TextureDesc gtaoTexDesc;
        //private readonly TextureHandle ssaoAccTex;
        
        private readonly ComputeShader gtaoShader;
        private readonly GTAOSettings settings;
        private readonly ConstantBuffer<GTAOConstants> gtaoConstantsBuffer;

        private readonly int 
            gtaoPrefilterDepthKernel, 
            gtaoMainKernel, 
            gtaoDenoiseKernel,
            gtaoDenoiseLastKernel;
        
        static GTAOPass() {
            gtaoTexDesc = TextureUtils.ColorTex(
                Vector2.one, TextureUtils.GetGraphicsFormat(RenderTextureFormat.ARGB32),
                "GTAO Tex"
            );
            gtaoTexDesc.clearBuffer = false;
            gtaoTexDesc.enableRandomWrite = true;
            
            filteredDepthDesc = new TextureDesc(Vector2.one) {
                colorFormat = TextureUtils.GetGraphicsFormat(RenderTextureFormat.RFloat),
                depthBufferBits = DepthBits.None,
                clearBuffer = false,
                enableRandomWrite = true,
                useMipMap = true,
                autoGenerateMips = false,
                slices = 5,
                name = "GTAO Prefiltered Depth"
            };

            edgesTexDesc = new TextureDesc(Vector2.one) {
                colorFormat = TextureUtils.GetGraphicsFormat(RenderTextureFormat.R8),
                depthBufferBits = DepthBits.None,
                clearBuffer = false,
                enableRandomWrite = true,
               // useMipMap = true,
                autoGenerateMips = false,
                //slices = 5,
                name = "GTAO Edges"
            };
        }

        public GTAOPass(Retrolight retrolight, GTAOSettings settings) : base(retrolight) {
            //ssaoAccTex = renderGraph.CreateSharedTexture(ssaoTexDesc);
            this.settings = settings;
            gtaoShader = ShaderBundle.Instance.GTAOShader;
            gtaoPrefilterDepthKernel = gtaoShader.FindKernel("PrefilterDepthPass");
            gtaoMainKernel = gtaoShader.FindKernel("GTAOUltra");
            gtaoDenoiseKernel = gtaoShader.FindKernel("DenoisePass");
            gtaoDenoiseLastKernel = gtaoShader.FindKernel("DenoiseLastPass");

            gtaoConstantsBuffer = new ConstantBuffer<GTAOConstants>();
        }
        
        private class GTAOPassData {
            public TextureHandle SrcDepth;
            public TextureHandle FilteredDepth;
            public TextureHandle WorkingAOTex;
            public TextureHandle WorkingEdges;
            public TextureHandle FinalAOTex;
            //public BufferHandle HilbertIndice
        }
        
        public TextureHandle Run(TextureHandle depthTex/*, BufferHandle hilbertIndices*/) {
            var builder = AddRenderPass("GTAO Pass", Render, out GTAOPassData passData);

            //builder.AllowPassCulling(false);
            passData.SrcDepth = builder.ReadTexture(depthTex);
            passData.FilteredDepth = builder.CreateTransientTexture(filteredDepthDesc);

            passData.WorkingAOTex = builder.CreateTransientTexture(gtaoTexDesc);
            passData.WorkingEdges = builder.CreateTransientTexture(edgesTexDesc);
            
            passData.FinalAOTex = builder.WriteTexture(renderGraph.CreateTexture(gtaoTexDesc));

            return passData.FinalAOTex;
        }

        private void Render(GTAOPassData passData, RenderGraphContext ctx) {
            var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            //Matrix4x4 invProjMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1)) * projMatrix.inverse;
            
            gtaoConstantsBuffer.UpdateData(ctx.cmd, GetGTAOConstants(projMatrix));
            gtaoConstantsBuffer.Set(gtaoShader, Shader.PropertyToID("GTAOConstantBuffer"));

            ctx.cmd.SetComputeTextureParam(
                gtaoShader, gtaoPrefilterDepthKernel,
                "SrcRawDepth", passData.SrcDepth
            );
            
            //todo: precompute name ids
            void SetDepthMipTarget(int i) => ctx.cmd.SetComputeTextureParam(
                gtaoShader, gtaoPrefilterDepthKernel, 
                $"OutWorkingDepthMip{i}", passData.FilteredDepth, i
            );
            for (int i = 0; i < 5; i++) SetDepthMipTarget(i);
            
            var tiles = viewportParams.TileCount;
            ctx.cmd.DispatchCompute(gtaoShader, gtaoPrefilterDepthKernel, tiles.x, tiles.y, 1);

            //run main GTAO pass
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoMainKernel, "SrcWorkingDepth", passData.FilteredDepth);
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoMainKernel, "OutWorkingEdges", passData.WorkingEdges);
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoMainKernel, "OutWorkingAOTerm", passData.WorkingAOTex);
            ctx.cmd.DispatchCompute(gtaoShader, gtaoMainKernel, tiles.x * 2, tiles.y * 2, 1);
            
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoDenoiseLastKernel, "SrcWorkingAOTerm", passData.WorkingAOTex);
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoDenoiseLastKernel, "SrcWorkingEdges", passData.WorkingEdges);
            ctx.cmd.SetComputeTextureParam(gtaoShader, gtaoDenoiseLastKernel, "OutFinalAOTerm", passData.FinalAOTex);
            ctx.cmd.DispatchCompute(gtaoShader, gtaoDenoiseLastKernel, tiles.x, tiles.y * 2, 1);
        }
        
        
        [StructLayout(LayoutKind.Sequential)]
        struct GTAOConstants {
            public Vector2Int ViewportSize;
            public Vector2 ViewportPixelSize;                  // .zw == 1.0 / ViewportSize.xy

            public Vector2 DepthUnpackConsts;
            public Vector2 CameraTanHalfFOV;

            public Vector2 NDCToViewMul;
            public Vector2 NDCToViewAdd;

            public Vector2 NDCToViewMul_x_PixelSize;
            public float EffectRadius;                       // world (viewspace) maximum size of the shadow
            public float EffectFalloffRange;

            public float RadiusMultiplier;
            public float Padding0;
            public float FinalValuePower;
            public float DenoiseBlurBeta;

            public float SampleDistributionPower;
            public float ThinOccluderCompensation;
            public float DepthMIPSamplingOffset;
            public int NoiseIndex;                         // frameIndex % 64 if using TAA or 0 otherwise
        }
        
        GTAOConstants GetGTAOConstants(Matrix4x4 projMatrix) {
            float depthLinearizeMul = -projMatrix.m23;
            float depthLinearizeAdd = projMatrix.m22;
            if (depthLinearizeMul * depthLinearizeAdd < 0) {
                depthLinearizeAdd = -depthLinearizeAdd;
            }

            float tanHalfFovX = 1f / projMatrix.m00;
            float tanHalfFovY = 1f / projMatrix.m11;

            Vector2 invResolution = new Vector2(viewportParams.Resolution.z, viewportParams.Resolution.w);
            Vector2 ndcToViewMul = new Vector2(2 * tanHalfFovX, -2 * tanHalfFovY);

            return new GTAOConstants {
                ViewportSize = viewportParams.PixelCount, 
                ViewportPixelSize = invResolution,
                DepthUnpackConsts = new Vector2(depthLinearizeMul, depthLinearizeAdd),
                
                CameraTanHalfFOV = new Vector2(tanHalfFovX, tanHalfFovY),
                
                NDCToViewMul = ndcToViewMul,
                NDCToViewAdd = new Vector2(-tanHalfFovX, tanHalfFovY),
                NDCToViewMul_x_PixelSize = new Vector2(
                    ndcToViewMul.x * invResolution.x, 
                    ndcToViewMul.y * invResolution.y
                ),
                
                EffectRadius = settings.Radius,
            };
        }
        
        public override void Dispose() {
            gtaoConstantsBuffer.Release();
            //renderGraph.ReleaseSharedTexture(ssaoAccTex);
        }
    }
}