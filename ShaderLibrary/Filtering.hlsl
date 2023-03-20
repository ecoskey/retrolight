#ifndef RETROLIGHT_DITHERING_INCLUDED
#define RETROLIGHT_DITHERING_INCLUDED

//bayer matrix values copied from Acerola on YT
static const int bayer2[2 * 2] = {
    0, 2,
    3, 1,
};

static const int bayer4[4 * 4] = {
    0,  8,  2,  10,
    12, 4,  14, 6,
    3,  11, 1,  9,
    15, 7,  13, 5
};

static const int bayer8[8 * 8] = {
    0,  32, 8,  40, 2,  34, 10, 42,
    48, 16, 56, 24, 50, 18, 58, 26,
    12, 44, 4,  36, 14, 46, 6,  38,
    60, 28, 52, 20, 62, 30, 54, 22,
    3,  35, 11, 43, 1,  33, 9,  41,
    51, 19, 59, 27, 49, 17, 57, 25,
    15, 47, 7,  39, 13, 45, 5,  37,
    63, 31, 55, 23, 61, 29, 53, 21
};

#define DITHER_TEMPLATE(Type, Dim, Dim2F) \
    Type Dither##Dim(Type value, Type spread, uint2 pos) { \
        uint2 matrixPos = pos % Dim; \
        uint matrixIndex = matrixPos.y * Dim + matrixPos.x; \
        const int matrixValue = bayer##Dim[matrixIndex]; \
        const real normMatrixValue = matrixValue / Dim2F - 0.5f; \
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

#endif