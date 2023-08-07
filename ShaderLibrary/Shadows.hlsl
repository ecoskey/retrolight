#ifndef RETROLIGHT_SHADOWS_INCLUDED
#define RETROLIGHT_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "LightingInput.hlsl"

#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

float SampleDirectionalShadowAtlas(float3 positionSTS) {
    return SAMPLE_TEXTURE2D_SHADOW(DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(float3 positionWS, float shadowStrength) {
    const float3 positionSTS = mul(DirectionalShadowMatrices[2], float4(positionWS, 1.0)).xyz;
    const float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, shadowStrength);
}

#endif