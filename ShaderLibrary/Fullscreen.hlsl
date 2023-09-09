#ifndef RETROLIGHT_FULLSCREEN_INCLUDED
#define RETROLIGHT_FULLSCREEN_INCLUDED

#include "Common.hlsl"

struct V2F {
    float4 positionCS : SV_Position;
    float2 uv : V2F_UV;
};

V2F FullscreenVertex(uint vertexId : VERTEXID_SEMANTIC) {
    V2F output;
    output.positionCS = GetFullScreenTriangleVertexPosition(vertexId);
    output.uv = GetFullScreenTriangleTexCoord(vertexId);
    #if UNITY_UV_STARTS_AT_TOP
    if (_ProjectionParams.x >= 0)
        output.uv.y = 1 - output.uv.y;
    #endif
    return output;
}

#endif
