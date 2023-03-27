using System;
using Data;
using Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using Util;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class Retrolight : RenderPipeline {
    internal RenderGraph RenderGraph { get; private set; }
    internal readonly ShaderBundle ShaderBundle;
        
    internal readonly int PixelRatio;
    internal readonly bool UsePostFx;
    internal readonly PostFxSettings PostFxSettings;
        
    internal FrameData FrameData { get; private set; }

    //render passes
    private SetupPass setupPass;
    private GBufferPass gBufferPass;
    private LightingPass lightingPass;
    private TransparentPass transparentPass;
    private PostFxPass postFxPass;
    private FinalPass finalPass;
        

    public Retrolight(
        ShaderBundle shaderBundle, int pixelRatio,
        bool usePostFx, PostFxSettings postFxSettings
    ) {
        //todo: enable SRP batcher, other graphics settings like linear light intensity
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
            
        RenderGraph = new RenderGraph("Retrolight Render Graph");
        //RenderGraph.RegisterDebug();
        ShaderBundle = shaderBundle;
            
        PixelRatio = pixelRatio;
        UsePostFx = usePostFx;
        PostFxSettings = postFxSettings;

        setupPass = new SetupPass(this);
        gBufferPass = new GBufferPass(this);
        lightingPass = new LightingPass(this);
        transparentPass = new TransparentPass(this);
        postFxPass = new PostFxPass(this);
        finalPass = new FinalPass(this);

        Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
        RTHandles.Initialize(Screen.width / PixelRatio, Screen.height / PixelRatio);
        //RTHandles.ResetReferenceSize(Screen.width / PixelRatio, Screen.height / PixelRatio);
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        BeginFrameRendering(context, cameras);
        foreach (var camera in cameras) {
            BeginCameraRendering(context, camera);
            RenderCamera(context, camera);
            EndCameraRendering(context, camera);
        }
        RenderGraph.EndFrame();
        EndFrameRendering(context, cameras);
    }

    private void RenderCamera(ScriptableRenderContext context, Camera camera) {
        if (!camera.TryGetCullingParameters(out var cullingParams)) return;
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        cullingParams.shadowDistance = camera.farClipPlane; //at least for orthographic mode
        //cullingParams.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
        //cullingParams.shadowDistance = 100; //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        CullingResults cull = context.Cull(ref cullingParams);
        RTHandles.SetReferenceSize(camera.pixelWidth / PixelRatio, camera.pixelHeight / PixelRatio);
        var viewportParams = new ViewportParams(RTHandles.rtHandleProperties);
        FrameData = new FrameData(camera, cull, viewportParams);

        using var snapContext = SnappingUtility.Snap(camera, camera.transform, viewportParams); //todo: move to FrameData

        context.SetupCameraProperties(camera);

        CommandBuffer cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
        var renderGraphParams = new RenderGraphParameters {
            scriptableRenderContext = context,
            commandBuffer = cmd,
            currentFrameIndex = Time.frameCount,
        };
            
        using (RenderGraph.RecordAndExecute(renderGraphParams)) {
            var lightInfo = setupPass.Run();
            var gBuffer = gBufferPass.Run();
            var lightingData = lightingPass.Run(gBuffer, lightInfo);
            transparentPass.Run(gBuffer, lightInfo, lightingData);
            postFxPass.Run(lightingData.FinalColorTex, PostFxSettings);
            //for post processing: don't run if scene view, and if scene view disables image fx
            //PostProcessPass -> writes to final color buffer after all other shaders
            finalPass.Run(lightingData.FinalColorTex, snapContext.ViewportShift);
        }
        

        switch (camera.clearFlags) {
            case CameraClearFlags.Skybox:  
                context.DrawSkybox(camera);
                break;
            case CameraClearFlags.Color:   break;
            case CameraClearFlags.Depth:   break;
            case CameraClearFlags.Nothing: break;
            default: throw new ArgumentOutOfRangeException();
        }
            
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
            
        #if UNITY_EDITOR //todo: is this the right way to do this?
        if (
            SceneView.currentDrawingSceneView is not null &&
            SceneView.currentDrawingSceneView.camera is not null && 
            SceneView.currentDrawingSceneView.camera == camera
        ) {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
        #endif
        context.Submit();
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;

        setupPass.Dispose();
        gBufferPass.Dispose();
        lightingPass.Dispose();
        transparentPass.Dispose();
        postFxPass.Dispose();
        finalPass.Dispose();

        setupPass = null;
        gBufferPass = null;
        lightingPass = null;
        postFxPass = null;
        transparentPass = null;
        finalPass = null;

        Blitter.Cleanup();
        //RenderGraph.UnRegisterDebug();
        RenderGraph.Cleanup();
        RenderGraph = null;
    }
}