using UnityEngine;
using UnityEngine.Rendering;

public static class Constants {
    public const int
        MaximumLights = 1024,
        MaxDirectionalShadows = 16,
        MaxDirectionalCascades = 4,
        MaxOtherShadows = 64;

    public const int
        SmallTile = 8,
        MediumTile = 16;

    public const int UIntBitSize = 32;

    public static readonly ShaderTagId
        GBufferPassId = new ShaderTagId("RetrolightGBuffer"),
        DecalPassId = new ShaderTagId("RetrolightDecal"),
        ForwardOpaquePassId = new ShaderTagId("RetrolightForwardOpaque"),
        ForwardTransparentPassId = new ShaderTagId("RetrolightForwardTransparent");

    public static readonly int
        ViewportParamsId = Shader.PropertyToID("ViewportParams"),
        LightCountId = Shader.PropertyToID("LightCount");

    public const string
        LightBufferName = "Lights",
        LightCullingResultsBufferName = "LightCullingResults";
        
    public static readonly int
        LightBufferId = Shader.PropertyToID(LightBufferName),
        LightCullingResultsId = Shader.PropertyToID(LightCullingResultsBufferName);
            
    public static readonly int
        DirectionalShadowAtlasId = Shader.PropertyToID("DirectionalShadowAtlas"),
        OtherShadowAtlasId = Shader.PropertyToID("OtherShadowAtlas"),
        DirectionalShadowMatricesId = Shader.PropertyToID("DirectionalShadowMatrices"),
        OtherShadowMatricesId = Shader.PropertyToID("OtherShadowMatrices");

    public const string
        DiffuseTexName = "DiffuseTex",
        SpecularTexName = "SpecularTex",
        DepthTexName = "DepthTex",
        NormalTexName = "NormalTex";

    public static readonly int
        DiffuseTexId = Shader.PropertyToID(DiffuseTexName),
        SpecularTexId = Shader.PropertyToID(SpecularTexName),
        DepthTexId = Shader.PropertyToID(DepthTexName),
        NormalTexId = Shader.PropertyToID(NormalTexName);

    public const string FinalColorTexName = "FinalColorTex";
    public static readonly int FinalColorTexId = Shader.PropertyToID(FinalColorTexName);
}