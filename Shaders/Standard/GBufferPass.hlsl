#ifndef RETROLIGHT_GBUFFER_PASS_INCLUDED
#define RETROLIGHT_GBUFFER_PASS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"
#include "Packages/net.cosc.retrolight/ShaderLibrary/Lighting.hlsl"
#include "Packages/net.cosc.retrolight/ShaderLibrary/GBufferOut.hlsl"

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

float3 GetNormalTS(float2 baseUV) {
    const float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_MainTex, baseUV);
    return UnpackNormalScale(map, _NormalScale);
}

V2F GBufferVertex(VertexInput input) {
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
    const float4 baseST = _MainTex_ST;
    output.uv = input.uv * baseST.xy + baseST.zw;
    return output; 
}

GBufferOut GBufferFragment(V2F input) {
    UNITY_SETUP_INSTANCE_ID(input);
    const float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 color = baseMap * _MainColor;
    #if defined(_Clipping)
    clip(color.a - _Cutoff);
    #endif
    const float3 normalWS = NormalTangentToWorld(GetNormalTS(input.uv), normalize(input.normalWS), input.tangentWS);
    const float3 normalVS = TransformWorldToViewNormal(normalWS, false);
    return GetGBufferOut(GetMetallicSurface(color.rgb, 1, normalVS, _Metallic, _Smoothness, _EdgeStrength));
}

#endif
