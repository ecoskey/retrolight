using Retrolight.Data;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Passes {
    public sealed class DefaultRenderProcedure : RenderProcedure {
        private readonly SetupPass setupPass;
        private readonly ZPrepass zPrepass;
        private readonly LightingPasses lightingPasses;
        private readonly GBufferPass gBufferPass;
        private readonly Option<GTAOPass> gtaoPass;
        private readonly ForwardPass forwardPass;
        private readonly UIPass uiPass;
        private readonly PostFxPasses postFxPasses;
        private readonly UtilPasses utilPasses;
        private readonly FinalPass finalPass;
        
        public DefaultRenderProcedure(Retrolight pipeline) : this(
            pipeline, 
            Option.None<ComputeShader>(),
            Option.None<GTAOSettings>()
        ) { }

        public DefaultRenderProcedure(
            Retrolight pipeline, 
            Option<ComputeShader> customLighting,
            Option<GTAOSettings> gtaoSettings
        ) : base(pipeline) {
            setupPass = new SetupPass(pipeline);
            zPrepass = new ZPrepass(pipeline); 
            lightingPasses = new LightingPasses(pipeline, customLighting);
            gBufferPass = new GBufferPass(pipeline);
            gtaoPass = gtaoSettings.Map(s => new GTAOPass(pipeline, s));
            forwardPass = new ForwardPass(pipeline);
            uiPass = new UIPass(pipeline);
            postFxPasses = new PostFxPasses(pipeline);
            utilPasses = new UtilPasses(pipeline);
            finalPass = new FinalPass(pipeline);
        }

        public override void Run(RenderGraph renderGraph, FrameData frameData) {
            using var snapContext = SnappingUtils.SnapCamera(frameData.Camera, frameData.ViewportParams); //todo: move to FrameData
            
            setupPass.Run();
            var lights = lightingPasses.AllocateLights();
            //var hilbertIndices = utilPasses.GetHilbertIndices();
            var depthTex = zPrepass.Run();
            var culledLights = lightingPasses.CullLights(depthTex, lights, false);

            var gBuffer = gBufferPass.RunWithZPrepass(depthTex);
            var ao = gtaoPass.Map(p => p.Run(depthTex));

            //var shadows = lightingPasses.RunShadows(lights);

            var sceneTex = lightingPasses.RunLighting(
                gBuffer, depthTex, lights, culledLights//, hilbertIndices,
                //Option.None<ShadowData>, Option.Some(ssao)
            );
            
            forwardPass.Run(gBuffer, depthTex, lights, culledLights, sceneTex, false);
            postFxPasses.Run(sceneTex);

            var ui = uiPass.Run();
            //todo: compositing and tonemapping pass
            var cameraTarget = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
            utilPasses.PixelPerfectUpscale(sceneTex, cameraTarget, true);
        }
        
        public override void Dispose() {
            setupPass.Dispose();
            zPrepass.Dispose();
            lightingPasses.Dispose();
            gBufferPass.Dispose();
            gtaoPass.Dispose();
            forwardPass.Dispose();
            uiPass.Dispose();
            postFxPasses.Dispose();
            utilPasses.Dispose();
            finalPass.Dispose();
        }
    }
}