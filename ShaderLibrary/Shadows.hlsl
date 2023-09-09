#ifndef RETROLIGHT_SHADOWS_INCLUDED
#define RETROLIGHT_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "LightingInput.hlsl"

#if defined(ENABLE_SHADOWS)
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);
#endif

float SampleDirectionalShadowAtlas(float3 positionSTS) {
    #if defined(ENABLE_SHADOWS)
    return SAMPLE_TEXTURE2D_SHADOW(DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
    #else
    return 1;
    #endif
}

float GetDirectionalShadowAttenuation(uint i, float3 positionWS, float shadowStrength) {
    #if defined(ENABLE_SHADOWS)
    const float3 positionSTS = mul(DirectionalShadowMatrices[i], float4(positionWS, 1.0)).xyz;
    const float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, shadowStrength);
    #else
    return 1;
    #endif
}

#endif