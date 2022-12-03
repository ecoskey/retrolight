#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(Albedo);
SAMPLER(sampler_Albedo);

TEXTURE2D(Depth);
SAMPLER(sampler_Depth);

TEXTURE2D(Normal);
SAMPLER(sampler_Normal);

float4 SampleAlbedo(float2 pos) {
    return SAMPLE_TEXTURE2D(Albedo, sampler_Albedo, pos);
}

float4 SampleAlbedoLOD(float2 pos, float lod) {
    return SAMPLE_TEXTURE2D_LOD(Albedo, sampler_Albedo, pos, lod);
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

//todo: look into shader variant stuff for reducing extra work calculated

float Sample01Depth(float2 pos) {
    float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_Depth, pos);
    #if ORTHOGRAPHIC_CAMERA
    return Ortho01Depth(rawDepth);
    #else
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
    #endif
}

float Sample01DepthLOD(float2 pos, float lod) {
    float rawDepth = SAMPLE_DEPTH_TEXTURE_LOD(Depth, sampler_Depth, pos, lod);
    #if ORTHOGRAPHIC_CAMERA
    return Ortho01Depth(rawDepth);
    #else
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
    #endif
}

float SampleEyeDepth(float2 pos) {
    float rawDepth = SAMPLE_DEPTH_TEXTURE(Depth, sampler_Depth, pos);
    #if ORTHOGRAPHIC_CAMERA
    return OrthoEyeDepth(rawDepth);
    #else
    return LinearEyeDepth(rawDepth, _ZBufferParams);
    #endif
}

float SampleEyeDepthLOD(float2 pos, float lod) {
    float rawDepth = SAMPLE_DEPTH_TEXTURE_LOD(Depth, sampler_Depth, pos, lod);
    #if ORTHOGRAPHIC_CAMERA
    return OrthoEyeDepth(rawDepth);
    #else
    return LinearEyeDepth(rawDepth, _ZBufferParams);
    #endif
}

float3 WorldSpaceFromDepth(float2 ndc) {
    float depth = Sample01Depth(ndc);
    float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 WorldSpaceFromDepthLOD(float2 ndc, float lod) {
    float depth = Sample01DepthLOD(ndc, lod);
    float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 SampleNormal(float2 pos) {
    return SAMPLE_TEXTURE2D(Normal, sampler_Normal, pos).rgb * 2 - 1;
}

float3 SampleNormalLOD(float2 pos, float lod) {
    return SAMPLE_TEXTURE2D_LOD(Normal, sampler_Normal, pos, lod).rgb * 2 - 1;
}

#endif