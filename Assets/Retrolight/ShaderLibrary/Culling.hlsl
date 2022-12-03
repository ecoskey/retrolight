#ifndef RETROLIGHT_CULLING_DEFINED
#define RETROLIGHT_CULLING_DEFINED

struct AABB {
    float3 min;
    float3 max;
};

struct Plane {
    float3 normal;
    float dist;
};

struct Frustum {
    Plane u;
    Plane d;
    Plane l;
    Plane r;
};

struct Sphere {
    float3 pos;
    float radius;
};



#endif