#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"
#include "Sampling.hlsl"

TEXTURE2D(DiffuseTex); // stores diffuse color & roughness
TEXTURE2D(SpecularTex); // stores specular color & effect edge strength
//TEXTURE2D(EmissionTex); // stores emission from emissive materials
TEXTURE2D(NormalTex); // stores world space normals and alpha (to separate background from foreground)
TEXTURE2D(DepthTex);

float4 SampleDiffuse(float2 uv) {
    return SAMPLE_TEXTURE2D(DiffuseTex, POINT_SAMPLER, uv);
}

float4 SampleDiffuseLevel(float2 uv, uint mip = 0) {
    return SAMPLE_TEXTURE2D_LOD(DiffuseTex, POINT_SAMPLER, uv, mip);
}

float4 LoadDiffuse(uint2 pos, uint mip = 0) {
    return LOAD_TEXTURE2D_LOD(DiffuseTex, pos, mip);
}

float4 SampleSpecular(float2 uv) {
    return SAMPLE_TEXTURE2D(SpecularTex, POINT_SAMPLER, uv);
}

float4 SampleSpecularLevel(float2 uv, uint mip = 0) {
    return SAMPLE_TEXTURE2D_LOD(SpecularTex, POINT_SAMPLER, uv, mip);
}

float4 LoadSpecular(uint2 pos, uint mip = 0) {
    return LOAD_TEXTURE2D_LOD(SpecularTex, pos, mip);
}

float3 SampleNormal(float2 uv) {
    return UnpackNormalOctQuadEncode(SAMPLE_TEXTURE2D(NormalTex, POINT_SAMPLER, uv).xy);
}

float3 SampleNormalLevel(float2 uv, uint mip = 0) {
    return UnpackNormalOctQuadEncode(SAMPLE_TEXTURE2D_LOD(NormalTex, POINT_SAMPLER, uv, mip).xy);
}

float3 LoadNormal(uint2 pos, uint mip = 0) {
    return UnpackNormalOctQuadEncode(LOAD_TEXTURE2D_LOD(NormalTex, pos, mip).xy);
}

float Ortho01Depth(float depth) {
    #if UNITY_REVERSED_Z
    return 1 - depth;
    #else
    return depth;
    #endif
}

float OrthoEyeDepth(float deviceDepth) {
    return lerp(_ProjectionParams.y, _ProjectionParams.z, Ortho01Depth(deviceDepth));
}

float Decode01Depth(float deviceDepth) {
    if (IS_ORTHOGRAPHIC_CAMERA) return Ortho01Depth(deviceDepth);
    return Linear01Depth(deviceDepth, _ZBufferParams);
}

float Sample01Depth(float2 uv) {
    const float deviceDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, POINT_SAMPLER, uv);
    return Decode01Depth(deviceDepth);
}

float Load01Depth(uint2 pos) {
    const float deviceDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    return Decode01Depth(deviceDepth);
}

float DecodeEyeDepth(float deviceDepth) {
    if (IS_ORTHOGRAPHIC_CAMERA) return OrthoEyeDepth(deviceDepth);
    return LinearEyeDepth(deviceDepth, _ZBufferParams);
}

float SampleEyeDepth(float2 uv) {
    const float deviceDepth = SAMPLE_DEPTH_TEXTURE(DepthTex, POINT_SAMPLER, uv);
    return DecodeEyeDepth(deviceDepth);
}

float LoadEyeDepth(uint2 pos) {
    const float rawDepth = LOAD_TEXTURE2D(DepthTex, pos).r;
    return DecodeEyeDepth(rawDepth);
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

float3 ViewSpaceFromDepthCompute(uint2 pos, float2 reciprocalResolution) {
    const float depth = LOAD_TEXTURE2D(DepthTex, pos).r;
    return ComputeViewSpacePosition(float2(pos) * reciprocalResolution, depth, UNITY_MATRIX_I_P);
}

#endif
