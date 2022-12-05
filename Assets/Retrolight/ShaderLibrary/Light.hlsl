#ifndef RETROLIGHT_LIGHT_INCLUDED
#define RETROLIGHT_LIGHT_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

#define MAX_LIGHTS 1024

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

//todo: look into utilities in Core RP lib Packing.hlsl 
struct Light {
    float3 position;
    uint type2_flags14_range16; //flags are currently unused, possibly layer mask?
    uint2 color48_angle16; //packed half3 color and half precision spot angle
    uint2 direction; //unused half of y component

    uint Type() {
        return type2_flags14_range16 & 0x03;
    }

    uint Flags() {
        return type2_flags14_range16 >> 2 & 0x3FFF ;
    }

    float Range() {
        return f16tof32(type2_flags14_range16 >> 16);
    }

    float3 Color() {
        return float3(
            f16tof32(color48_angle16.x),
            f16tof32(color48_angle16.x >> 16),
            f16tof32(color48_angle16.y)
        );
    }

    float Angle() {
        return f16tof32(color48_angle16.y >> 16);
    }

    float3 Direction() {
        return float3(
            f16tof32(direction.x),
            f16tof32(direction.x >> 16),
            f16tof32(direction.y)
        );
    }
};

Light DirectionalLight(float3 color, float3 direction) {
    Light light;
    light.position = float3(0, 0, 0);
    light.type2_flags14_range16 = DIRECTIONAL_LIGHT;
    light.color48_angle16 = PackFloat3(color);
    light.direction = PackFloat3(direction);
    return light;
}

Light PointLight(float3 position, float3 color, float range) {
    Light light;
    light.position = position;
    light.type2_flags14_range16 = POINT_LIGHT | f32tof16(range) << 16;
    light.color48_angle16 = PackFloat3(color);
    light.direction = uint2(0, 0);
    return light;
}

#endif