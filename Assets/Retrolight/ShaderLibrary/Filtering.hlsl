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

float Dither2(float value, float spread, uint2 pos) {
    uint2 matrixPos = pos % 2;
    uint matrixIndex = matrixPos.y * 2 + matrixPos.x;
    const int matrixValue = bayer2[matrixIndex];
    const float normMatrixValue = matrixValue / 4.0f - 0.5f;
    return value + normMatrixValue * spread;
}

float Dither4(float value, float spread, uint2 pos) {
    uint2 matrixPos = pos % 4;
    uint matrixIndex = matrixPos.y * 4 + matrixPos.x;
    const int matrixValue = bayer4[matrixIndex];
    const float normMatrixValue = matrixValue / 16.0f - 0.5f;
    return value + normMatrixValue * spread;
}

float Dither8(float value, float spread, uint2 pos) {
    uint2 matrixPos = pos % 8;
    uint matrixIndex = matrixPos.y * 8 + matrixPos.x;
    const int matrixValue = bayer8[matrixIndex];
    const float normMatrixValue = matrixValue / 64.0f - 0.5f;
    return value + normMatrixValue * spread;
}

float Quantize(float value, int steps) {
    return floor(value * (steps - 1) + 0.5) / (steps - 1);
}

#endif