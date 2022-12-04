#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#define MAX_DIRECTIONAL_LIGHTS 4
#define MAX_POINT_LIGHTS 1024

struct DirectionalLight {
    float3 direction;
    float3 color;
};

#endif