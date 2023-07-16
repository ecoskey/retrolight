#ifndef RETROLIGHT_GBUFFER_PASS_INCLUDED
#define RETROLIGHT_GBUFFER_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(float, _NormalScale)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
    UNITY_DEFINE_INSTANCED_PROP(float, _EdgeStrength)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct VertexInput {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F {
    float4 positionCS : SV_Position;
    float3 normalWS : V2F_Normal;
    float4 tangentWS : V2F_Tangent;
    float2 uv : V2F_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GBufferOut {
    float4 diffuse : SV_Target0;
    float4 specular : SV_Target1;
    float3 normal : SV_Target2;
};

float3 GetNormalTS(float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_MainTex, baseUV);
    float scale = ACCESS_PROP(_NormalScale);
    float3 normal = DecodeNormal(map, scale);
    return normal;
}

V2F GBufferVertex(VertexInput input)
{
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
    float4 baseST = ACCESS_PROP(_MainTex_ST);
    output.uv = input.uv * baseST.xy + baseST.zw;
    return output;
}

GBufferOut GBufferFragment(V2F input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    const float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    const float4 baseColor = ACCESS_PROP(_MainColor);
    float4 color = baseMap * baseColor;
    clip(color.a - ACCESS_PROP(_Cutoff));

    const float3 normal = normalize(NormalTangentToWorld(GetNormalTS(input.uv), input.normalWS, input.tangentWS));

    Surface surface = GetMetallicSurface(
        color.rgb, 1, normal, ACCESS_PROP(_Metallic),
        ACCESS_PROP(_Smoothness), ACCESS_PROP(_EdgeStrength)
    );

    GBufferOut output;
    output.diffuse = float4(surface.baseDiffuse, surface.roughness);
    output.specular = float4(surface.baseSpecular, surface.edgeStrength);
    output.normal = (surface.normal + 1) / 2;
    
    return output;
}

#endif
