#ifndef RETROLIGHT_SHADOWS_INCLUDED
#define RETROLIGHT_SHADOWS_INCLUDED

#include "Common.hlsl"

TEXTURE2D_SHADOW(DirectionalShadowAtlas);
TEXTURE2D_SHADOW(OtherShadowAtlas);

#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

#endif