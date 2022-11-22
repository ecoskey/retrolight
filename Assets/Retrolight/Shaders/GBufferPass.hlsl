#ifndef RETROLIGHT_GBUFFER_PASS_INCLUDED
#define RETROLIGHT_GBUFFER_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor);
    UNITY_DEFINE_INSTANCED_PROP(float4,_MainTex_ST);
    UNITY_DEFINE_INSTANCED_PROP(float, _NormalScale);
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff);
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic);
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness);
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define InputProp(prop) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, prop)

struct Attributes {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F {
    float4 positionCS : SV_POSITION;
    float3 normalWS : V2F_NORMAL;
    float2 uv : V2F_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GBufferOut {
    float3 albedo : SV_TARGET0;
    float3 normal : SV_Target1;
};

V2F GBufferVertex(Attributes input) {
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 baseST = InputProp(_MainTex_ST);
    output.uv = input.uv * baseST.xy + baseST.zw;
    return output;
}

GBufferOut GBufferFragment(V2F input) {
    UNITY_SETUP_INSTANCE_ID(input);
    GBufferOut output;
    
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 baseColor = InputProp(_MainColor);
    float4 color = baseMap * baseColor;
    clip(color.a - InputProp(_Cutoff));
    output.albedo = color.rgb;

    output.normal = input.normalWS;
    return output;
}

#endif