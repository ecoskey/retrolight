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
    float intensity;
    switch (light.Type()) {
        case DIRECTIONAL_LIGHT:
            intensity = saturate(dot(surface.normal, light.Direction()));
            return Quantize(Dither8(intensity, 0.05, pos), 6) * light.Color();
        case POINT_LIGHT:
            /*float3 dist = light.position - surface.position;
            intensity = saturate(dot(surface.normal, normalize(dist)));
            float attenuation = light.Range() / Length2(dist);
            return intensity * attenuation * light.Color();*/
        case SPOT_LIGHT:
        default:
            return 0;
    }
    
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
    //return specularStrength;
    return IncomingLight(surface, light, pos) * baseLitColor;
}

#endif