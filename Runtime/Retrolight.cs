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
    internal readonly bool AllowPostFx;
    private readonly bool allowHDR;

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
        bool allowPostFx, bool allowHDR
    ) {
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
            
        RenderGraph = new RenderGraph("Retrolight Render Graph");
        //RenderGraph.RegisterDebug();
        ShaderBundle = shaderBundle;
            
        PixelRatio = pixelRatio;
        AllowPostFx = allowPostFx;
        this.allowHDR = allowHDR;

        setupPass = new SetupPass(this);
        gBufferPass = new GBufferPass(this);
        lightingPass = new LightingPass(this);
        transparentPass = new TransparentPass(this);
        postFxPass = new PostFxPass(this);
        finalPass = new FinalPass(this);

        RTHandles.Initialize(1, 1); //todo: what is this even
        Debug.LogWarning("REMBER TO FIX BLIT SHADER, ISN'T REALLY NECESSARY BUT YOU SHOULD");
        Blitter.Initialize(shaderBundle.BlitShader, shaderBundle.BlitWithDepthShader);
    }

    protected override void Render(ScriptableRenderContext ctx, Camera[] cameras) {
        BeginFrameRendering(ctx, cameras);
        foreach (var camera in cameras) {
            BeginCameraRendering(ctx, camera);
            RenderCamera(ctx, camera);
            EndCameraRendering(ctx, camera);
        }
        RenderGraph.EndFrame();
        EndFrameRendering(ctx, cameras);
    }

    private void RenderCamera(ScriptableRenderContext ctx, Camera camera) {
        if (!camera.TryGetCullingParameters(out var cullingParams)) return;
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        cullingParams.shadowDistance = camera.farClipPlane; //at least for orthographic mode
        //cullingParams.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
        //cullingParams.shadowDistance = 100; //TODO: SET THIS FROM CONFIG PLEASE PLEASE PLEASE PLEASE PLEASE
        CullingResults cull = ctx.Cull(ref cullingParams);
        RTHandles.SetReferenceSize(camera.pixelWidth / PixelRatio, camera.pixelHeight / PixelRatio);
        var viewportParams = new ViewportParams(RTHandles.rtHandleProperties);
        FrameData = new FrameData(camera, cull, viewportParams, allowHDR);

        using var snapContext = SnappingUtil.SnapCamera(camera, viewportParams); //todo: move to FrameData

        ctx.SetupCameraProperties(camera);

        CommandBuffer cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
        var renderGraphParams = new RenderGraphParameters {
            scriptableRenderContext = ctx,
            commandBuffer = cmd,
            currentFrameIndex = Time.frameCount,
        };

        var skyboxRenderer = ctx.CreateSkyboxRendererList(camera);
            
        using (RenderGraph.RecordAndExecute(renderGraphParams)) {
            var lightInfo = setupPass.Run();
            var gBuffer = gBufferPass.Run();
            var lightingData = lightingPass.Run(gBuffer, lightInfo, skyboxRenderer);
            //transparentPass.Run(gBuffer, lightInfo, lightingData);
            
            postFxPass.Run(lightingData.FinalColorTex);
            
            finalPass.Run(lightingData.FinalColorTex, snapContext.ViewportShift);
        }
        

        /*switch (camera.clearFlags) {
            case CameraClearFlags.Skybox:  
                ctx.DrawSkybox(camera);
                break;
            case CameraClearFlags.Color:   break;
            case CameraClearFlags.Depth:   break;
            case CameraClearFlags.Nothing: break;
            default: throw new ArgumentOutOfRangeException();
        }*/
            
        ctx.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
            
        #if UNITY_EDITOR //todo: is this the right way to do this?
        if (
            camera.cameraType == CameraType.SceneView &&
            !SceneView.currentDrawingSceneView.sceneViewState.showImageEffects
        ) {
            ctx.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            ctx.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
        #endif
        ctx.Submit();
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;
        
        Blitter.Cleanup();

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
        
        //RenderGraph.UnRegisterDebug();
        RenderGraph.Cleanup();
        RenderGraph = null;
    }
}