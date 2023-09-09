#ifndef RETROLIGHT_LIGHT_INCLUDED
#define RETROLIGHT_LIGHT_INCLUDED

#include "Common.hlsl"

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

#define F_LIGHT_SHADOWED 1

//todo: look into utilities in Core RP lib Packing.hlsl
//todo: make sure this packs correctly
struct Light {
    float3 positionVS;
    uint type2_flags6_shadowIndex8_range16; //flags are currently unused, possibly layer mask?

    #if REAL_IS_HALF
        half3 color;
        half cosAngle;
        half3 direction;
        half shadowStrength;
    #else
        uint2 color48_cosAngle16; //packed half3 color and half precision cosine of stlight angle
        uint2 direction48_shadowStrength16; //packed half3 direction and half precision normalized strength of shadow
    #endif

    uint Type() {
        return type2_flags6_shadowIndex8_range16 & 0x03;
    }

    uint Flags() {
        return type2_flags6_shadowIndex8_range16 >> 2 & 0x3F;
    }

    uint ShadowIndex() {
        return type2_flags6_shadowIndex8_range16 >> 8 & 0xFF;
    }

    real Range() {
        return f16tof32(type2_flags6_shadowIndex8_range16 >> 16);
    }

    real3 Color() {
        #if REAL_IS_HALF
            return color;
        #else
            return float3(
                f16tof32(color48_cosAngle16.x),
                f16tof32(color48_cosAngle16.x >> 16),
                f16tof32(color48_cosAngle16.y)
            );
        #endif
    }

    real CosAngle() {
        #if REAL_IS_HALF
            return cosAngle;
        #else
            return f16tof32(color48_cosAngle16.y >> 16);
        #endif
    }

    real3 Direction() {
        #if REAL_IS_HALF
            return direction;
        #else
            return float3(
                f16tof32(direction48_shadowStrength16.x),
                f16tof32(direction48_shadowStrength16.x >> 16),
                f16tof32(direction48_shadowStrength16.y)
            );
        #endif
    } 
    
    real ShadowStrength() {
        #if REAL_IS_HALF
            return shadowStrength
        #else
            return f16tof32(direction48_shadowStrength16.y >> 16);
        #endif
    }
};

#endif
