#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(Albedo);
TEXTURE2D(Depth);
TEXTURE2D(Normal);
TEXTURE2D(Attributes);

SAMPLER(sampler_PointClamp);

float4 SampleAlbedo(float2 uv) {
    return SAMPLE_TEXTURE2D(Albedo, sampler_PointClamp, uv);
}

float4 LoadAlbedo(uint2 pos) {
    return LOAD_TEXTURE2D(Albedo, pos);
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
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_PointClamp, uv);
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float Load01Depth(uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(Depth, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float SampleEyeDepth(float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_PointClamp, uv);
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float LoadEyeDepth(uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(Depth, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float3 SampleNormal(float2 uv) {
    return SAMPLE_TEXTURE2D(Normal, sampler_PointClamp, uv).rgb * 2 - 1;
}

float3 LoadNormal(uint2 pos) {
    return LOAD_TEXTURE2D(Normal, pos).rgb * 2 - 1;
}

float4 SampleAttributes(float2 uv) {
    return SAMPLE_TEXTURE2D(Attributes, sampler_PointClamp, uv);
}

float4 LoadAttributes(uint2 pos) {
    return LOAD_TEXTURE2D(Attributes, pos);
}

/*float3 WorldSpaceFromDepth(float2 ndc) {
    const float depth = Sample01Depth(ndc);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 WorldSpaceFromDepthLOD(float2 ndc, float lod) {
    const float depth = Sample01DepthLOD(ndc, lod);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}*/

#endif