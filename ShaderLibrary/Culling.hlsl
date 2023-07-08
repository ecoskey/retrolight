#ifndef RETROLIGHT_CULLING_DEFINED
#define RETROLIGHT_CULLING_DEFINED

#include "Light.hlsl"

#define BUCKET_SIZE 32
#define TILE_BUCKET_COUNT(Num) ((Num + BUCKET_SIZE - 1) / BUCKET_SIZE)

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
    float r;
};

struct Cone {
    
};

struct Capsule {
    float3 a;
    float3 b;
    float r;
};

Sphere PointLightCullVolume(Light light) {
    Sphere volume;
    volume.pos = light.position;
    volume.r = light.Range();
    return volume;
}

Capsule LineLightCullVolume(Light light) {
    Capsule volume;
    volume.a = light.position;
    volume.b = light.position + light.Direction() * light.Range();
    volume.r = light.CosAngle();
    return volume;
}

float2 UVToScreenSpaceXY(float2 uv) {
    return unity_OrthoParams.xy * (uv * 2 - 1);
}

void TransformSphereToView(inout Sphere volume) {
    volume.pos = TransformWorldToView(volume.pos);
}


AABB OrthoVolumeFromUVDepth(float2 minUv, float2 maxUv, float minDepth, float maxDepth) {
    AABB volume;
    volume.min = float3(UVToScreenSpaceXY(minUv), minDepth);
    volume.max = float3(UVToScreenSpaceXY(maxUv), maxDepth);
    return volume;
}

float3 ClosestPointOnAABB(AABB aabb, float3 pos) {
    return clamp(pos, aabb.min, aabb.max);
}

bool SphereIntersectsAABB(Sphere sphere, AABB aabb) {
    float3 closestPoint = ClosestPointOnAABB(aabb, sphere.pos);
    float3 dist = sphere.pos - closestPoint;
    return (dist.x * dist.x + dist.y * dist.y + dist.z * dist.z) < (sphere.r * sphere.r);
}

#endif
