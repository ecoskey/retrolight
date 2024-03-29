#ifndef RETROLIGHT_VIEWPORT_INCLUDED
#define RETROLIGHT_VIEWPORT_INCLUDED

#if TILING_SMALL
    #define TILE_SIZE 8
#elif TILING_MED
    #define TILE_SIZE 16
#else
    #define TILE_SIZE 16
#endif

CBUFFER_START(ViewportParams)
float4 Resolution;
uint2 PixelCount;
uint2 TileCount;
float2 ViewportScale;
CBUFFER_END

int4 UVToTile(float2 uv) {
    const int2 pixelPos = uv * PixelCount;
    int4 result;
    result.xy = pixelPos / TILE_SIZE;
    result.zw = pixelPos % TILE_SIZE;
    return result;
}

uint TileIndex(uint2 groupId) {
    return groupId.y * TileCount.x + groupId.x;
}

bool IsPixelOOB(int2 pos, int2 vpSize) {
    return any(pos < 0 || pos >= vpSize);
}

bool IsPixelOOB(int2 pos) {
    return IsPixelOOB(pos, PixelCount);
}

uint2 ClampPixelPos(int2 pos, uint2 viewportSize) {
    return clamp(pos, uint2(0, 0), viewportSize);
}

uint2 ClampPixelPos(int2 pos) {
    return ClampPixelPos(pos, PixelCount);
}

float2 PixelToNDC(uint2 pos, float2 invResolution) {
    return (float2(pos) + 0.5) * invResolution;
}

float2 PixelToNDC(uint2 pos) {
    return PixelToNDC(pos, Resolution.zw);
}

#endif
