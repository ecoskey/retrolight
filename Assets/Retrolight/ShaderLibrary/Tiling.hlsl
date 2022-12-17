#ifndef RETROLIGHT_TILING_INCLUDED
#define RETROLIGHT_TILING_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

#define TILE_SIZE 8 // tiles are 8x8

CBUFFER_START(TilingData)
uint2 TileCount;
uint2 PixelResolution;
float4 Resolution;
CBUFFER_END

int4 UVToTile(float2 uv)
{
    int2 pixelPos = uv * PixelResolution.xy;
    int4 result;
    result.xy = pixelPos / TILE_SIZE;
    result.zw = pixelPos % TILE_SIZE;
    return result;
}

uint TileIndex(uint2 groupId)
{
    return groupId.y * TileCount.x + groupId.x;
}

#endif
