#ifndef RETROLIGHT_UNLIT_PASS_INCLUDED
#define RETROLIGHT_UNLIT_PASS_INCLUDED

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

V2F UnlitVertex(VertexInput input) {
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    const float4 baseST = _MainTex_ST;
    output.uv = input.uv * baseST.xy + baseST.zw;
    return output;
}

float4 UnlitFragment(V2F input) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);
    const float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    return baseMap * _MainColor;
}

#endif
