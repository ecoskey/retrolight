#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(_Albedo);
SAMPLER(sampler_Albedo);

TEXTURE2D(_Depth);
SAMPLER(sampler_Depth);

TEXTURE2D(_Normal);
SAMPLER(sampler_Normal);

float4 SampleAlbedo(float2 pos) {
    return SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, pos);
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
    float rawDepth = SAMPLE_TEXTURE2D(_Depth, sampler_Depth, pos).r;
    float perspDepth = Linear01DepthFromNear(rawDepth, _ZBufferParams);
    float orthoDepth = Ortho01Depth(rawDepth);

    return lerp(perspDepth, orthoDepth, unity_OrthoParams.w);
}

float SampleEyeDepth(float2 pos) {
    float rawDepth = SAMPLE_TEXTURE2D(_Depth, sampler_Depth, pos).r;
    float perspDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
    float orthoDepth = OrthoEyeDepth(rawDepth);

    return lerp(perspDepth, orthoDepth, unity_OrthoParams.w);
}

float3 WorldSpaceFromNDC(float2 ndc) {
    float depth = Sample01Depth(ndc);
    float remappedDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    return ComputeWorldSpacePosition(ndc, remappedDepth, UNITY_MATRIX_I_VP);
}

float3 SampleNormal(float2 pos) {
    return SAMPLE_TEXTURE2D(_Normal, sampler_Normal, pos).rgb * 2 - 1;
}

#endif