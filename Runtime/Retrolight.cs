using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public sealed class Retrolight : RenderPipeline {
    internal RenderGraph RenderGraph { get; private set; }
    private RenderProcedure renderProcedure;
        
    internal readonly int PixelRatio; //todo: add to global/per camera settings????
    private readonly bool allowPostFx;
    private readonly bool allowHDR;

    internal readonly ShadowSettings ShadowSettings;
    
    private FrameData frameData;
    internal FrameData FrameData {
        private set => frameData = value;
        get => isRenderingFrame ? frameData : throw new InvalidOperationException(
            "Attempted to access Retrolight.FrameData outside of the frame rendering cycle."
        );
    }
    private bool isRenderingFrame = false;
    
    public Retrolight(
        int pixelRatio, bool allowPostFx, bool allowHDR, 
        ShadowSettings shadowSettings, Func<Retrolight, RenderProcedure> renderProcedureFn
    ) {
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
            
        RenderGraph = new RenderGraph("Retrolight Render Graph");
        renderProcedure = renderProcedureFn(this);
            
        PixelRatio = pixelRatio;
        this.allowPostFx = allowPostFx;
        this.allowHDR = allowHDR;
        ShadowSettings = shadowSettings;

        RTHandles.Initialize(1, 1); //todo: what is this even
        Debug.LogWarning("REMBER TO FIX BLIT SHADER, ISN'T REALLY NECESSARY BUT YOU SHOULD");
        Blitter.Initialize(ShaderBundle.Instance.BlitShader, ShaderBundle.Instance.BlitWithDepthShader);
    }

    protected override void Render(ScriptableRenderContext ctx, Camera[] cameras) => Render(ctx, cameras.ToList());


    protected override void Render(ScriptableRenderContext ctx, List<Camera> cameras) {
        BeginContextRendering(ctx, cameras);
        isRenderingFrame = true;
        foreach (var camera in cameras) {
            BeginCameraRendering(ctx, camera);
            RenderCamera(ctx, camera);
            EndCameraRendering(ctx, camera);
        }
        isRenderingFrame = false;
        RenderGraph.EndFrame();
        EndContextRendering(ctx, cameras);
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
        ViewportParams viewportParams = new ViewportParams(RTHandles.rtHandleProperties);
        FrameData = new FrameData(camera, cull, viewportParams, allowPostFx, allowHDR); //todo: pass in post fx settings
        
        CommandBuffer cmd = CommandBufferPool.Get("Execute Retrolight Render Graph");
        var renderGraphParams = new RenderGraphParameters {
            scriptableRenderContext = ctx,
            commandBuffer = cmd,
            currentFrameIndex = Time.frameCount,
        };

        using (RenderGraph.RecordAndExecute(renderGraphParams)) {
            renderProcedure.Run(RenderGraph, frameData);
        }
        
        ctx.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        ctx.Submit();
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;
        
        Blitter.Cleanup();
        
        renderProcedure.Dispose();
        renderProcedure = null;
        
        //RenderGraph.UnRegisterDebug();
        RenderGraph.Cleanup();
        RenderGraph = null;
    }
}