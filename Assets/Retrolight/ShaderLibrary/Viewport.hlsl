#ifndef RETROLIGHT_VIEWPORT_INCLUDED
#define RETROLIGHT_VIEWPORT_INCLUDED

#define TILE_SIZE 8 // tiles are 8x8

CBUFFER_START(ViewportParams)
float4 Resolution;
uint2 PixelCount;
uint2 TileCount;
float2 ViewportScale;
CBUFFER_END

int4 UVToTile(float2 uv) {
    int2 pixelPos = uv * PixelCount.xy;
    int4 result;
    result.xy = pixelPos / TILE_SIZE;
    result.zw = pixelPos % TILE_SIZE;
    return result;
}

uint TileIndex(uint2 groupId) {
    return groupId.y * TileCount.x + groupId.x;
}

bool IsPixelOOB(uint2 pos) {
    return any(pos < 0 || pos >= PixelCount);
}

uint2 ClampPixelPos(int2 pos) {
    return clamp(pos, uint2(0, 0), PixelCount);
}

#endif
