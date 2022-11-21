#ifndef RETROLIGHT_GBUFFER_INCLUDED
#define RETROLIGHT_GBUFFER_INCLUDED

#include "Common.hlsl"

TEXTURE2D(Albedo);
SAMPLER(sampler_Albedo);

TEXTURE2D(Depth);
SAMPLER(sampler_Depth);

TEXTURE2D(Normals);
SAMPLER(sampler_Normals);

float4 resolution;

float4 SampleAlbedo(float2 pos) {
    return SAMPLE_TEXTURE2D(Albedo, sampler_Albedo, pos);
}

float SampleDepth(float2 pos) { //todo: meters or just sample actual depth
    return SAMPLE_TEXTURE2D(Depth, sampler_Depth, pos);
}

float SampleNormals(float2 pos) {
    return UnpackNormal(SAMPLE_TEXTURE2D(Normals, sampler_Normals, pos));
}

#endif