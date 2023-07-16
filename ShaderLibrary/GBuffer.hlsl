#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"
#include "Samplers.hlsl"

TEXTURE2D(DiffuseTex); // stores diffuse color & roughness
TEXTURE2D(SpecularTex); // stores specular color & effect edge strength
//TEXTURE2D(EmissionTex); // stores emission from emissive materials
TEXTURE2D(NormalTex);
TEXTURE2D(DepthTex);

float4 SampleDiffuse(const float2 uv) {
    return SAMPLE_TEXTURE2D_LOD(DiffuseTex, POINT_SAMPLER, uv, 0);
}

float4 LoadDiffuse(const uint2 pos) {
    return LOAD_TEXTURE2D_LOD(DiffuseTex, pos, 0);
}

float4 SampleSpecular(const float2 uv) {
    return SAMPLE_TEXTURE2D_LOD(SpecularTex, POINT_SAMPLER, uv, 0);
}

float4 LoadSpecular(const uint2 pos) {
    return LOAD_TEXTURE2D(SpecularTex, pos);
}

float3 SampleNormal(const float2 uv) {
    return normalize(SAMPLE_TEXTURE2D_LOD(NormalTex, POINT_SAMPLER, uv, 0).rgb * 2 - 1);
}

float3 LoadNormal(const uint2 pos) {
    return normalize(LOAD_TEXTURE2D(NormalTex, pos).rgb * 2 - 1);
}

float Ortho01Depth(const float depth) {
    #if UNITY_REVERSED_Z
    return 1 - depth;
    #else
    return depth;
#endif
}

float OrthoEyeDepth(const float depth) {
    return lerp(_ProjectionParams.y, _ProjectionParams.z, Ortho01Depth(depth));
}

float Sample01Depth(const float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, POINT_SAMPLER, uv);
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01Depth(rawDepth, _ZBufferParams);
}

float Load01Depth(const uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return Ortho01Depth(rawDepth);
    return Linear01DepthFromNear(rawDepth, _ZBufferParams);
}

float SampleEyeDepth(const float2 uv) {
    const float rawDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, POINT_SAMPLER, uv);
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float LoadEyeDepth(const uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    if (ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(rawDepth);
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}

float3 WorldSpaceFromDepth(const float2 ndc) {
    const float depth = Sample01Depth(ndc);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 WorldSpaceFromDepthCompute(const uint2 pos, const float2 reciprocalResolution) {
    const float depth = Load01Depth(pos);
    const float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(float2(pos) * reciprocalResolution, remappedDepth, UNITY_MATRIX_I_VP);
}

#endif
