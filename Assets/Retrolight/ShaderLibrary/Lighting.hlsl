#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#include "Common.hlsl"
#include "Light.hlsl"
#include "Filtering.hlsl"

struct Surface {
    float3 position;
    float3 color;
    float3 normal;
    float3 viewDir;

    float alpha;
    float metallic;
    float smoothness;
    float depthEdgeStrength;
    float normalEdgeStrength;
};

struct BrdfParams {
    float3 baseDiffuse;
    float3 baseSpecular;
    float roughness;
};

float3 GetViewDirection(float3 pos) {
    return normalize(_WorldSpaceCameraPos - pos); //doesn't work for orthographic cameras
}

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic) {
    const float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BrdfParams GetBrdfParams(Surface surface/*, bool applyAlphaToDiffuse = false*/) {
    BrdfParams params;
    const float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    params.baseDiffuse = surface.color * oneMinusReflectivity;
    /*if (applyAlphaToDiffuse) {
        brdf.diffuse *= surface.alpha;
    }*/
    params.baseSpecular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    const float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    params.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return params;
}

float3 IncomingLight(Surface surface, Light light, uint2 pos) {
    float intensity = saturate(dot(surface.normal, light.Direction()));
    return intensity * light.Color(); //integrate shadow/angle attenuation
}

float3 DirectBRDF(Surface surface, BrdfParams params, Light light, uint2 pos) {
    //return surface.normal;
    //return IncomingLight(surface, light);
    const float3 h = SafeNormalize(light.Direction() + surface.viewDir);
    const float nh2 = Sq(saturate(dot(surface.normal, h)));
    const float lh2 = Sq(saturate(dot(light.Direction(), h)));
    const float r2 = Sq(params.roughness);
    const float d2 = Sq(float(nh2 * (r2 - 1.0) + 1.00001)); //todo: check if this is right
    const float normalization = params.roughness * 4.0 + 2.0;
    const float specularStrength = r2 / (d2 * max(0.1, lh2) * normalization);
    const float3 baseLitColor = specularStrength * params.baseSpecular + params.baseDiffuse;
    return IncomingLight(surface, light, pos) * baseLitColor;
}

#endif