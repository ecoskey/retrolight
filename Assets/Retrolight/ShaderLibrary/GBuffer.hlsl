#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(Albedo);
SAMPLER(sampler_Albedo);

TEXTURE2D(Depth);
SAMPLER(sampler_Depth);

TEXTURE2D(Normal);
SAMPLER(sampler_Normal);

TEXTURE2D(Attributes);
SAMPLER(sampler_Attributes);

float4 SampleAlbedo(float2 uv) {
    return SAMPLE_TEXTURE2D(Albedo, sampler_Albedo, uv);
}

float4 SampleAlbedoLOD(float2 uv, float lod) {
    return SAMPLE_TEXTURE2D_LOD(Albedo, sampler_Albedo, uv, lod);
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
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_Depth, uv);
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float Sample01DepthLOD(float2 uv, float lod) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE_LOD(Depth, sampler_Depth, uv, lod);
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float SampleEyeDepth(float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_Depth, uv);
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float SampleEyeDepthLOD(float2 uv, float lod) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE_LOD(Depth, sampler_Depth, uv, lod);
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float3 SampleNormal(float2 uv) {
    return SAMPLE_TEXTURE2D(Normal, sampler_Normal, uv).rgb * 2 - 1;
}

float3 SampleNormalLOD(float2 uv, float lod) {
    return SAMPLE_TEXTURE2D_LOD(Normal, sampler_Normal, uv, lod).rgb * 2 - 1;
}

float4 SampleAttributes(float2 uv) {
    return SAMPLE_TEXTURE2D(Attributes, sampler_Attributes, uv);
}

float4 SampleAttributesLOD(float2 uv, float lod) {
    return SAMPLE_TEXTURE2D_LOD(Attributes, sampler_Attributes, uv, lod);
}

float3 WorldSpaceFromDepth(float2 ndc) {
    const float depth = Sample01Depth(ndc);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 WorldSpaceFromDepthLOD(float2 ndc, float lod) {
    const float depth = Sample01DepthLOD(ndc, lod);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

#endif