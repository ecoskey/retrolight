#ifndef RETROLIGHT_EDGES_INCLUDED
#define RETROLIGHT_EDGES_INCLUDED

#include "Common.hlsl"
#include "GBuffer.hlsl"
#include "Viewport.hlsl"

float LoadRelativeEyeDepth(int2 pos, int2 offset) {
    return LoadEyeDepth(ClampPixelPos(pos + offset));
}

float3 LoadRelativeNormal(int2 pos, int2 offset) {
    return LoadNormal(ClampPixelPos(pos + offset));
}

float GetDepthEdgeIndicator(int2 pos, float depth) {
    float diff = 0;
    diff += clamp(LoadRelativeEyeDepth(pos, int2(1, 0)) - depth, 0, 1);
    diff += clamp(LoadRelativeEyeDepth(pos, int2(-1, 0)) - depth, 0, 1);
    diff += clamp(LoadRelativeEyeDepth(pos, int2(0, 1)) - depth, 0, 1);
    diff += clamp(LoadRelativeEyeDepth(pos, int2(0, -1)) - depth, 0, 1);
    return floor(smoothstep(.1, .2, diff) * 2) / 2;
}

float NeighborNormalEdgeIndicator(int2 pos, const int2 offset, float depth, float3 normal) {
    const float depthDiff = LoadRelativeEyeDepth(pos, offset) - depth;
    const float3 neighborNormal = LoadRelativeNormal(pos, offset);
    
    // Edge pixels should yield to faces who's normals are closer to the bias normal.
    const float3 normalEdgeBias = float3(1, 1, 1);
    const float normalDiff = dot(normal - neighborNormal, normalEdgeBias);
    const float normalIndicator = saturate(smoothstep(-.01, .01, normalDiff));

    // Only the shallower pixel should detect the normal edge.
    const float depthIndicator = saturate(sign(depthDiff * .25 + .0025));

    return (1 - dot(normal, neighborNormal)) * depthIndicator * normalIndicator;
}

float GetNormalEdgeIndicator(int2 pos, float depth, float3 normal) {
    float indicator = 0;
    indicator += NeighborNormalEdgeIndicator(pos, int2(1 ,0), depth, normal);
    indicator += NeighborNormalEdgeIndicator(pos, int2(-1 ,0), depth, normal);
    indicator += NeighborNormalEdgeIndicator(pos, int2(0 ,1), depth, normal);
    indicator += NeighborNormalEdgeIndicator(pos, int2(0 ,-1), depth, normal);
    return step(0.1, indicator);
}

float2 GetEdgeStrength(int2 pos) {
    const float2 edgeStrengths = LoadAttributes(pos).zw;
    const float depthEdgeStrength = edgeStrengths.x;
    const float normalEdgeStrength = edgeStrengths.y;

    float depth;
    float3 normal;
    
    if (depthEdgeStrength > 0 || normalEdgeStrength > 0) {
        depth = LoadEyeDepth(pos);
        normal = LoadNormal(pos);
    }

    float dei = 0;
    if (depthEdgeStrength > 0) {
        dei = GetDepthEdgeIndicator(pos, depth);
    }

    float nei = 0;
    if (normalEdgeStrength > 0) {
        nei = GetNormalEdgeIndicator(pos, depth, normal);
    }
    
    return float2(dei, nei);
    //return edgeStrengths;
}

#endif