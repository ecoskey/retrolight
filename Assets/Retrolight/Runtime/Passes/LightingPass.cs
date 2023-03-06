using System;
using Retrolight.Data;
using Retrolight.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Runtime.Passes {
    public class LightingPass : RenderPass<LightingPass.LightingPassData> {

        private static readonly int finalColorTexId = Shader.PropertyToID("FinalColorTex");

        private readonly int lightingKernelId;

        public class LightingPassData {
            public TextureHandle FinalColorTex;
        }

        public LightingPass(Retrolight pipeline) : base(pipeline) {
            lightingKernelId = shaderBundle.LightingShader.FindKernel("Lighting");
        }

        protected override string PassName => "Lighting Pass";

        public TextureHandle Run(GBuffer gBuffer, LightingData lightingData) {
            using var builder = CreatePass(out var passData);
            gBuffer.ReadAll(builder);
            lightingData.ReadAll(builder);

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
            
            //context.cmd.SetGlobalTexture("DirectionalShadowAtlas", passData.ShadowAtlas);

            /*
               }
            }*/

            // LIGHTS
            var tileCount = viewportParams.TileCount;

            context.cmd.SetComputeTextureParam(
                shaderBundle.LightingShader, lightingKernelId, 
                finalColorTexId, passData.FinalColorTex
            );
            context.cmd.DispatchCompute(
                shaderBundle.LightingShader, lightingKernelId,
                tileCount.x, tileCount.y, 1
            );
        }
    }
}