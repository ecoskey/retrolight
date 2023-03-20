#ifndef RETROLIGHT_SHADOWS_INCLUDED
#define RETROLIGHT_SHADOWS_INCLUDED

#include "Common.hlsl"

#define MAX_DIRECTIONAL_SHADOWS 16
#define MAX_OTHER_SHADOWS 64

TEXTURE2D_SHADOW(DirectionalShadowAtlas);
TEXTURE2D_SHADOW(OtherShadowAtlas);
float4x4 DirectionalShadowMatrices[MAX_DIRECTIONAL_SHADOWS];
float4x4 OtherShadowMatrices[MAX_OTHER_SHADOWS];

#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

float SampleDirectionalShadowAtlas(float3 positionSTS) {
    return SAMPLE_TEXTURE2D_SHADOW(DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(float3 positionWS, float shadowStrength) {
    const float3 positionSTS = mul(DirectionalShadowMatrices[0],float4(positionWS, 1.0)).xyz;
    const float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, shadowStrength);
}

#endif