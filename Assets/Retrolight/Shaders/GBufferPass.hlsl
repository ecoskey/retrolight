#ifndef RETROLIGHT_GBUFFER_PASS_INCLUDED
#define RETROLIGHT_GBUFFER_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

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
UNITY_DEFINE_INSTANCED_PROP(float, _DepthEdgeStrength)
UNITY_DEFINE_INSTANCED_PROP(float, _NormalEdgeStrength)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define ACCESS_PROP(prop) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, prop)

struct VertexInput
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F
{
    float4 positionCS : SV_Position;
    float3 normalWS : V2F_Normal;
    float4 tangentWS : V2F_Tangent;
    float2 uv : V2F_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GBufferOut
{
    float4 albedo : SV_Target0;
    float4 normal : SV_Target1;
    float4 attributes : SV_Target2;
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
    GBufferOut output;
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 baseColor = ACCESS_PROP(_MainColor);
    float4 color = baseMap * baseColor;
    clip(color.a - ACCESS_PROP(_Cutoff));
    output.albedo = color;

    float3 normal = NormalTangentToWorld(GetNormalTS(input.uv), input.normalWS, input.tangentWS);
    float3 normNorm = normalize(normal);
    output.normal = float4((normNorm + 1) / 2, 1);

    output.attributes = float4(
        ACCESS_PROP(_Metallic),
        ACCESS_PROP(_Smoothness),
        ACCESS_PROP(_DepthEdgeStrength),
        ACCESS_PROP(_NormalEdgeStrength)
    );

    return output;
}

#endif
