#ifndef RETROLIGHT_CULLING_DEFINED
#define RETROLIGHT_CULLING_DEFINED

#include "Light.hlsl"

namespace Culling {
    // ReSharper disable once CppInconsistentNaming
    struct AABB {
        float3 min;
        float3 max;
    };

    struct Plane {
        float3 normal;
        float dist;
    };

    struct Frustum {
        Plane planes[6]; // near, far, up, down, left, right;
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

    Sphere PointLightVolume(Light light) {
        Sphere volume;
        volume.pos = light.positionVS;
        volume.r = light.Range();
        return volume;
    }

    /*Capsule LineLightCullVolume(Light light) {
        Capsule volume;
        volume.a = light.position;
        volume.b = light.position + light.Direction() * light.Range();
        volume.r = light.CosAngle();
        return volume;
    }*/
    
    float3 ClosestPointAABB(AABB aabb, float3 pos) {
        return clamp(pos, aabb.min, aabb.max);
    }
    
    float DistToPlane(Plane plane, float3 pos) {
        return dot(plane.normal, pos) - plane.dist;
    }

    float3 ClosestPointPlane(Plane plane, float3 pos) {
        const float distToPlane = DistToPlane(plane, pos);
        return pos - distToPlane * plane.normal;
    }

    bool AABBvsAABB(AABB a, AABB b) {
        return all(a.min <= b.max && a.max >= b.min);
    }

    bool SphereVsAABB(Sphere sphere, AABB aabb) {
        const float3 closestPoint = ClosestPointAABB(aabb, sphere.pos);
        const float3 dist = sphere.pos - closestPoint;
        return Length2(dist) < Sq(sphere.r);
    }

    bool SphereVsPlane(Sphere sphere, Plane plane) {
        return Sq(DistToPlane(plane, sphere.pos)) + Sq(sphere.r) >= 0;
    }

    bool SphereVsFrustum(Sphere sphere, Frustum frustum) {
        //take constant calculation out of loop
        const float rad2 = Sq(sphere.r);
        
        UNITY_UNROLLX(6)
        for (int i = 0; i < 6; i++)
            if (Sq(DistToPlane(frustum.planes[i], sphere.pos)) + rad2 >= 0) return true;

        return false;
    }

    
}

#endif
