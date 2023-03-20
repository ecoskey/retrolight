#ifndef RETROLIGHT_TRANSPARENT_PASS_DEFINED
#define RETROLIGHT_TRANSPARENT_PASS_DEFINED

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

float3 GetNormalTS(const float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_MainTex, baseUV);
    float scale = ACCESS_PROP(_NormalScale);
    float3 normal = DecodeNormal(map, scale);
    return normal;
}

V2F TransparentVertex(const VertexInput input)
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

float4 TransparentFragment(const V2F input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 baseColor = ACCESS_PROP(_MainColor);
    float4 color = baseMap * baseColor;

    float3 normal = NormalTangentToWorld(GetNormalTS(input.uv), input.normalWS, input.tangentWS);
    float3 normNorm = normalize(normal);

    return color;
}

#endif