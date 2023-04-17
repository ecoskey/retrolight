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

#define FILTER_ARGS TEXTURE2D_PARAM(tex, smp), float2 uv, uint lod, float2 texelSize

#define SEPARABLE_FILTER_X_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_X(FILTER_ARGS) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
        	float offset = Offsets[i] * /*2.0 * */texelSize.x; \
        	color += SAMPLE_TEXTURE2D_LOD(tex, smp, uv + float2(offset, 0.0), lod).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_FILTER_Y_TEMPLATE(Name, Size, Offsets, Weights) \
    float4 Name##_Y(FILTER_ARGS) { \
        float3 color = 0.0; \
        UNITY_UNROLLX(Size) \
        for (int i = 0; i < Size; i++) { \
            float offset = Offsets[i] * /*2.0 * */texelSize.y; \
            color += SAMPLE_TEXTURE2D_LOD(tex, smp, uv + float2(0.0, offset), lod).rgb * Weights[i]; \
        } \
        return float4(color, 1); \
    } \

#define SEPARABLE_FILTER_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_FILTER_X_TEMPLATE(Name, Size, Offsets, Weights) \
    SEPARABLE_FILTER_Y_TEMPLATE(Name, Size, Offsets, Weights)

static const float gaussianOptimizedOffsets9[5] = { -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923 };
static const float gaussianOptimizedWeights9[5] = { 0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027 };

SEPARABLE_FILTER_TEMPLATE(Gaussian9, 5, gaussianOptimizedOffsets9, gaussianOptimizedWeights9)

static const float boxOffsets3[3] = { -1, 0, 1 };
static const float boxWeights3[3] = { 0.33333333, 0.33333333, 0.33333333 };

SEPARABLE_FILTER_TEMPLATE(Box3, 3, boxOffsets3, boxWeights3)

static const float boxOffsets5[5] = { -2, -1, 0, 1, 2 };
static const float boxWeights5[5] = { 0.2, 0.2, 0.2, 0.2, 0.2 };

SEPARABLE_FILTER_TEMPLATE(Box5, 5, boxOffsets5, boxWeights5)

static const float boxOffsets7[7] = { -3, -2, -1, 0, 1, 2, 3 };
static const float boxWeights7[7] = { 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571, 0.1428571 };

SEPARABLE_FILTER_TEMPLATE(Box7, 7, boxOffsets7, boxWeights7)


/*static const float tentOffsets3[3] = { -1, 0, 1 };
static const float tentWeights3[3] = { 0.25, 0.5, 0.25 };

SEPARABLE_FILTER_TEMPLATE(Tent3, 3, tentOffsets3, tentWeights3)*/

#define SAMPLE_WITH_OFFSET(dx, dy) SAMPLE_TEXTURE2D_LOD(tex, smp, uv + float2(dx, dy) * texelSize, 0)

float4 Tent9(FILTER_ARGS, bool preserveAlpha = false) {
    // Center
    float4 center = SAMPLE_WITH_OFFSET(0, 0) * 4;
    
    float4 result = center;
    result += SAMPLE_WITH_OFFSET(-1, -1);
    result += SAMPLE_WITH_OFFSET(0, -1) * 2;
    result += SAMPLE_WITH_OFFSET(1, -1);
    result += SAMPLE_WITH_OFFSET(-1, 0) * 2;
    result += SAMPLE_WITH_OFFSET(1, 0) * 2;
    result += SAMPLE_WITH_OFFSET(-1, 1);
    result += SAMPLE_WITH_OFFSET(0, 1) * 2;
    result += SAMPLE_WITH_OFFSET(1, 1);

    result /= 16.0;
    if (preserveAlpha) result.a = center.a;

    return result;
}

//refer to slideshow linked on https://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
float4 CustomBox13(FILTER_ARGS, bool preserveAlpha = false) {
    
    //texelSize *= 0.5; //<---- in TheCherno's code, but doesn't seem to match the actual reference?

    const float4 center = SAMPLE_TEXTURE2D_LOD(tex, smp, uv, 0);

    //inner box, arranged left-right, up-down
    const float4 a = SAMPLE_WITH_OFFSET(-1, -1);
    const float4 b = SAMPLE_WITH_OFFSET(1, -1);
    const float4 c = SAMPLE_WITH_OFFSET(-1, 1);
    const float4 d = SAMPLE_WITH_OFFSET(1, 1);

    //outer box, arranged left-right, up-down
    const float4 e = SAMPLE_WITH_OFFSET(-2, -2);
    const float4 f = SAMPLE_WITH_OFFSET(0, -2);
    const float4 g = SAMPLE_WITH_OFFSET(2, 2);
    
    const float4 h = SAMPLE_WITH_OFFSET(-2, 0);
    const float4 i = SAMPLE_WITH_OFFSET(2, 0);
    
    const float4 j = SAMPLE_WITH_OFFSET(-2, 2);
    const float4 k = SAMPLE_WITH_OFFSET(0, 2);
    const float4 l = SAMPLE_WITH_OFFSET(2, 2);

    float4 result = 0;
    result += 0.5 * (a + b + c + d);
    result += 0.125 * (e + f + h + center);
    result += 0.125 * (f + g + center + i);
    result += 0.125 * (h + center + j + k);
    result += 0.125 * (center + i + k + l);
    result *= 0.25;

    if (preserveAlpha) result.a = center.a;
    return result;
}

//from catlikeCoding
float4 BloomThreshold(float4 color, float4 thresholdParams) {
    float brightness = Max3(color.r, color.g, color.b);
    float soft = brightness + thresholdParams.y;
    soft = clamp(soft, 0.0, thresholdParams.z);
    soft = soft * soft * thresholdParams.w;
    float contribution = max(soft, brightness - thresholdParams.x);
    contribution /= max(brightness, 0.00001);
    return color * contribution;
}


#endif