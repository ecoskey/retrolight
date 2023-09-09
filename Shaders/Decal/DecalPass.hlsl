#ifndef RETROLIGHT_DECAL_PASS_INCLUDED
#define RETROLIGHT_DECAL_PASS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/GBuffer.hlsl"
#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

struct VertexInput {
    float3 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F {
    float4 positionCS : SV_Position;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GBufferOut {
    float4 diffuse : SV_Target0;
    //float4 normal : SV_Target1;
    //float4 attributes : SV_Target2;
};

/*float3 GetNormalTS(float2 baseUV)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_MainTex, baseUV);
    float scale = ACCESS_PROP(_NormalScale);
    float3 normal = DecodeNormal(map, scale);
    return normal;
}*/

V2F DecalVertex(VertexInput input) {
    V2F output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    return output;
}

GBufferOut DecalFragment(V2F input) {
    GBufferOut output;
    UNITY_SETUP_INSTANCE_ID(input);
    const float2 screenUV = (input.positionCS.xy + 1) / 2;
    const float3 positionWS = WorldSpaceFromDepth(screenUV);
    const float3 positionOS = TransformWorldToObject(positionWS);
    const float2 uv = positionOS.xy;

    //clip(all(positionOS.xy >= 0 || positionOS.xy <= 1));

    const float3 colorWS = SampleDiffuse(screenUV);

    const float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    const float4 color = baseMap * _MainColor;
    output.diffuse = float4(1, 1, 1, 1);
    //output.albedo = color + float4(colorWS, 1 - color.a);

    /*float3 normalWS = SampleNormal(screenUV);

    float3 normal = NormalTangentToWorld(GetNormalTS(uv), normalWS, input.tangentWS);
    float3 normNorm = normalize(normal);
    output.normal = float4((normNorm + 1) / 2, 1);*/

    return output;
}
#endif
