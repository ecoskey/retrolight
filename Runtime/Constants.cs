using UnityEngine;
using UnityEngine.Rendering;

public static class Constants {
    public const int 
        MaximumLights = 1024,
        MaxDirectionalShadows = 16,
        MaxOtherShadows = 64;

    public static readonly ShaderTagId 
        GBufferPassId = new ShaderTagId("RetrolightGBuffer"),
        DecalPassId = new ShaderTagId("RetrolightDecal"),
        TransparentPassId = new ShaderTagId("RetrolightTransparent");

    public static readonly int
        ViewportParamsId = Shader.PropertyToID("ViewportParams"),
        LightCountId = Shader.PropertyToID("LightCount");

    public const string
        LightBufferName = "Lights",
        CullingResultsBufferName = "CullingResults";
        
    public static readonly int
        LightBufferId = Shader.PropertyToID(LightBufferName),
        CullingResultsId = Shader.PropertyToID(CullingResultsBufferName);
            
    public static readonly int
        DirectionalShadowAtlasId = Shader.PropertyToID("DirectionalShadowAtlas"),
        OtherShadowAtlasId = Shader.PropertyToID("OtherShadowAtlas"),
        DirectionalShadowMatricesId = Shader.PropertyToID("DirectionalShadowMatrices"),
        OtherShadowMatricesId = Shader.PropertyToID("OtherShadowMatrices");
        
    public const string
        AlbedoTexName = "AlbedoTex",
        DepthTexName = "DepthTex",
        NormalTexName = "NormalTex",
        AttributesTexName = "AttributesTex";

    public static readonly int
        AlbedoTexId = Shader.PropertyToID(AlbedoTexName),
        DepthTexId = Shader.PropertyToID(DepthTexName),
        NormalTexId = Shader.PropertyToID(NormalTexName),
        AttributesTexId = Shader.PropertyToID(AttributesTexName);

    public const string FinalColorTexName = "FinalColorTex";
    public static readonly int FinalColorTexId = Shader.PropertyToID(FinalColorTexName);
}