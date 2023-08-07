using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

namespace Passes {
    public sealed class DefaultRenderProcedure : RenderProcedure {
        private readonly SetupPass setupPass;
        private readonly LightingPasses lightingPasses;
        private readonly GBufferPass gBufferPass;
        private readonly SsaoPass ssaoPass;
        private readonly PostFxPasses postFxPasses;
        private readonly FinalPass finalPass;

        public DefaultRenderProcedure(Retrolight pipeline, ComputeShader lightingModel = null) : base(pipeline) {
            setupPass = new SetupPass(pipeline);
            lightingPasses = new LightingPasses(pipeline, lightingModel);
            gBufferPass = new GBufferPass(pipeline);
            ssaoPass = new SsaoPass(pipeline);
            postFxPasses = new PostFxPasses(pipeline);
            finalPass = new FinalPass(pipeline);
        }

        public override void Run(RenderGraph renderGraph, FrameData frameData) {
            using var snapContext = SnappingUtils.SnapCamera(frameData.Camera, frameData.ViewportParams); //todo: move to FrameData
            
            setupPass.Run();
            var lights = lightingPasses.AllocateLights();
            
            var gBuffer = gBufferPass.Run(); 
            //var ssao = ssaoPass.Run(gBuffer);
            
            var shadows = lightingPasses.RunShadows(lights);
            var culledLights = lightingPasses.CullLights(gBuffer, lights);
            var finalColorTex = lightingPasses.RunLighting(gBuffer, lights, culledLights, shadows);
            //transparentPass.Run(gBuffer, lightInfo, lightingData);
            
            postFxPasses.Run(finalColorTex);
            
            finalPass.Run(finalColorTex, snapContext.ViewportShift);
        }
        
        public override void Dispose() {
            setupPass.Dispose();
            lightingPasses.Dispose();
            gBufferPass.Dispose();
            ssaoPass.Dispose();
            postFxPasses.Dispose();
            finalPass.Dispose();
        }
    }
}