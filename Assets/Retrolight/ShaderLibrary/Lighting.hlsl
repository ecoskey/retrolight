#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#define MAX_DIRECTIONAL_LIGHTS 

struct DirectionalLight {
    float3 direction;
    float3 color;
};

#endif