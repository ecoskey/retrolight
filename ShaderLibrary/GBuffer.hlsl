#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(AlbedoTex);
TEXTURE2D(DepthTex);
TEXTURE2D(NormalTex);
TEXTURE2D(AttributesTex);

float4 SampleAlbedo(float2 uv) {
    return SAMPLE_TEXTURE2D_LOD(AlbedoTex, DEFAULT_SAMPLER, uv, 0);
}

float4 LoadAlbedo(uint2 pos) {
    return LOAD_TEXTURE2D_LOD(AlbedoTex, pos, 0);
}

float Ortho01Depth(float depth) {
    #if UNITY_REVERSED_Z
    return 1 - depth;
    #else
    return depth;
    #endif
}

float OrthoEyeDepth(float depth) {
    return lerp(_ProjectionParams.y, _ProjectionParams.z, Ortho01Depth(depth));
}

float Sample01Depth(float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, DEFAULT_SAMPLER, uv);
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01Depth(rawDepth, _ZBufferParams);
}

float Load01Depth(uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float SampleEyeDepth(float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, DEFAULT_SAMPLER, uv);
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float LoadEyeDepth(uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float3 SampleNormal(float2 uv) {
    return normalize(SAMPLE_TEXTURE2D_LOD(NormalTex, DEFAULT_SAMPLER, uv, 0).rgb * 2 - 1);
}

float3 LoadNormal(uint2 pos) {
    return normalize(LOAD_TEXTURE2D(NormalTex, pos).rgb * 2 - 1);
}

float4 SampleAttributes(float2 uv) {
    return SAMPLE_TEXTURE2D_LOD(AttributesTex, DEFAULT_SAMPLER, uv, 0);
}

float4 LoadAttributes(uint2 pos) {
    return LOAD_TEXTURE2D(AttributesTex, pos);
}

float3 WorldSpaceFromDepth(float2 ndc) {
    const float depth = Sample01Depth(ndc);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 WorldSpaceFromDepthCompute(uint2 pos, float2 reciprocalResolution) {
    const float depth = Load01Depth(pos);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(float2(pos) * reciprocalResolution, remappedDepth, UNITY_MATRIX_I_VP);
}

#endif
