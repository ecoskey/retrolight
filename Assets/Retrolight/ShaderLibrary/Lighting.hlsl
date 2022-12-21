#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

struct Surface {
    float3 position;
    float3 normal;
    float3 viewDir;
    float alpha;
    float metallic;
    float smoothness;
    float depthEdgeStrength;
    float normalEdgeStrength;
};

#endif