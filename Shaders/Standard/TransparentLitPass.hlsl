#ifndef RETROLIGHT_TRANSPARENT_LIT_PASS_INCLUDED
#define RETROLIGHT_TRANSPARENT_LIT_PASS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

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

float3 GetNormalTS(const float2 baseUV) {
    const float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_MainTex, baseUV);
    return UnpackNormalScale(map, _NormalScale);
}

V2F TransparentVertex(VertexInput input) {
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

//todo: add light loop
float4 TransparentFragment(V2F input) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);
    const float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 color = baseMap * _MainColor;

    const float3 normal = normalize(NormalTangentToWorld(GetNormalTS(input.uv), input.normalWS, input.tangentWS));

    //PositionInputs positionInputs = GetPositionInput(input.positionCS.xy, Resolution.zw, input.positionCS.z, input.positionCS.w, )
    return color;
}

#endif
