#ifndef RETROLIGHT_DEPTH_ONLY_PASS_INCLUDED
#define RETROLIGHT_DEPTH_ONLY_PASS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

struct VertexInput {
    float3 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F {
    float4 positionCS : SV_Position;
    float2 uv : V2F_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

V2F DepthOnlyVertex(VertexInput input) {
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    float4 baseST = _MainTex_ST;
    output.uv = input.uv * baseST.xy + baseST.zw;
    return output;
}

void DepthOnlyFragment(V2F input) {
    #if defined(_Clipping)
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 baseColor = _MainColor;
    float4 color = baseMap * baseColor;
    clip(color.a - _Cutoff);
    #endif
}

#endif
