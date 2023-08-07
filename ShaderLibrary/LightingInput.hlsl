#ifndef RETROLIGHT_LIGHTING_INPUT_INCLUDED
#define RETROLIGHT_LIGHTING_INPUT_INCLUDED

#include "Light.hlsl"

#define MAX_LIGHTS 1024

#define MAX_DIRECTIONAL_SHADOWS 16
#define MAX_OTHER_SHADOWS 64
#define MAX_CASCADES 4

#define BUCKET_SIZE 32
#define TILE_BUCKET_COUNT(Num) ((Num + BUCKET_SIZE - 1) / BUCKET_SIZE)
#define LIGHT_TILE_BUCKET_COUNT TILE_BUCKET_COUNT(MAX_LIGHTS)

CBUFFER_START(LightingInputs)
uint LightCount;
uint CascadeCount;
float4x4 DirectionalShadowMatrices[MAX_DIRECTIONAL_SHADOWS];
float4x4 OtherShadowMatrices[MAX_OTHER_SHADOWS];
CBUFFER_END

StructuredBuffer<Light> Lights;
ByteAddressBuffer LightCullingResults;

TEXTURE2D_SHADOW(DirectionalShadowAtlas);
TEXTURE2D_SHADOW(OtherShadowAtlas);

#endif