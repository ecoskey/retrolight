#ifndef RETROLIGHT_DITHERING_INCLUDED
#define RETROLIGHT_DITHERING_INCLUDED

#include "Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

//bayer matrix values copied from Acerola on YT
static const int bayer2[4] = {
    0, 2,
    3, 1,
};

static const int bayer4[16] = {
    0,  8,  2,  10,
    12, 4,  14, 6,
    3,  11, 1,  9,
    15, 7,  13, 5
};

static const int bayer8[64] = {
    0,  32, 8,  40, 2,  34, 10, 42,
    48, 16, 56, 24, 50, 18, 58, 26,
    12, 44, 4,  36, 14, 46, 6,  38,
    60, 28, 52, 20, 62, 30, 54, 22,
    3,  35, 11, 43, 1,  33, 9,  41,
    51, 19, 59, 27, 49, 17, 57, 25,
    15, 47, 7,  39, 13, 45, 5,  37,
    63, 31, 55, 23, 61, 29, 53, 21
};

#define DITHER_TEMPLATE(Type, Size, Size2F) \
    Type Dither##Size(Type value, Type spread, uint2 pos) { \
        uint2 matrixPos = pos % Size; \
        uint matrixIndex = matrixPos.y * Size + matrixPos.x; \
        const int matrixValue = bayer##Size[matrixIndex]; \
        const real normMatrixValue = matrixValue / Size2F - 0.5f; \
        return value + normMatrixValue * spread; \
    }

#define DITHER_TEMPLATE_ALL(Type) \
    DITHER_TEMPLATE(Type, 2, 4.0f) \
    DITHER_TEMPLATE(Type, 4, 16.0f) \
    DITHER_TEMPLATE(Type, 8, 64.0f)

#define QUANTIZE_TEMPLATE(Type) \
    Type Quantize(Type value, int steps) { \
        return floor(value * (steps - 1) + 0.5) / (steps - 1); \
    }

DITHER_TEMPLATE_ALL(real)
DITHER_TEMPLATE_ALL(real2)
DITHER_TEMPLATE_ALL(real3)
DITHER_TEMPLATE_ALL(real4)

QUANTIZE_TEMPLATE(real)
QUANTIZE_TEMPLATE(real2)
QUANTIZE_TEMPLATE(real3)
QUANTIZE_TEMPLATE(real4)

#define SEPARABLE_BLUR_X_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_X(TEXTURE2D_PARAM(tex, smp), float2 uv, uint mipLevel, float texelSize) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
        	float offset = Offsets[i] * /*2.0 * */texelSize; \
        	color += SAMPLE_TEXTURE2D_LOD(tex, smp, uv + float2(offset, 0.0), mipLevel).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_BLUR_Y_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_Y(TEXTURE2D_PARAM(tex, smp), float2 uv, uint mipLevel, float texelSize) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
            float offset = Offsets[i] * /*2.0 * */texelSize; \
            color += SAMPLE_TEXTURE2D_LOD(tex, smp, uv + float2(0.0, offset), mipLevel).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_BLUR_COMPUTE_X_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_X(TEXTURE2D(tex), int2 pos, uint mipLevel, float texelSize) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
            int offset = Offsets[i] * /*2.0 * */texelSize; \
            color += SAMPLE_TEXTURE2D_LOD(tex, pos + int2(offset, 0.0), mipLevel).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_BLUR_COMPUTE_Y_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_Y(TEXTURE2D(tex), int2 pos, uint mipLevel, float texelSize) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
            int offset = Offsets[i] * /*2.0 * */texelSize; \
            color += LOAD_TEXTURE2D_LOD(tex, pos + int2(0.0, offset), mipLevel).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_BLUR_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_BLUR_X_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_BLUR_Y_TEMPLATE(Name, Size, Offsets, Weights)

#define SEPARABLE_BLUR_COMPUTE_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_BLUR_COMPUTE_X_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_BLUR_COMPUTE_Y_TEMPLATE(Name, Size, Offsets, Weights)

static const float gaussianOptimizedOffsets9[5] = { -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923 };
static const float gaussianOptimizedWeights9[5] = { 0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027 };

SEPARABLE_BLUR_TEMPLATE(gaussianBlur9, 5, gaussianOptimizedOffsets9, gaussianOptimizedWeights9)

float gaussianOffsets9[] = {
    -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0
};
float gaussianWeights9[] = {
    0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
    0.19459459, 0.12162162, 0.05405405, 0.01621622
};

SEPARABLE_BLUR_COMPUTE_TEMPLATE(gaussianBlur9_Compute, 9, gaussianOffsets9, gaussianWeights9)

static const float boxOffsets3[3] = { -1, 0, 1 };
static const float boxWeights3[3] = { 0.33333333, 0.33333333, 0.33333333 };

SEPARABLE_BLUR_TEMPLATE(boxBlur3, 3, boxOffsets3, boxWeights3)

static const float boxOffsets5[5] = { -2, -1, 0, 1, 2 };
static const float boxWeights5[5] = { 0.2, 0.2, 0.2, 0.2, 0.2 };

SEPARABLE_BLUR_TEMPLATE(boxBlur5, 5, boxOffsets5, boxWeights5)

static const float boxOffsets7[7] = { -3, -2, -1, 0, 1, 2, 3 };
static const float boxWeights7[7] = { 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571 };

SEPARABLE_BLUR_TEMPLATE(boxBlur7, 7, boxOffsets7, boxWeights7)

#endif