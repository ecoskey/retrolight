#ifndef RETROLIGHT_DECAL_PASS_INCLUDED
#define RETROLIGHT_DECAL_PASS_INCLUDED

#include "../ShaderLibrary/GBuffer.hlsl"

TEXTURE2D(SourceTex);
SAMPLER(sampler_SourceTex);

struct VertexInput {
    float3 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F {
    float4 positionCS : SV_Position;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GBufferOut {
    float4 albedo : SV_Target0;
    float4 normal : SV_Target1;
    float4 attributes : SV_Target2;
};

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
    const float3 positionWS = WorldSpaceFromDepth()
}
#endif