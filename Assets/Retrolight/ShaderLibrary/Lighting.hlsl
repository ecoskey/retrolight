#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#include "Common.hlsl"
#include "Light.hlsl"

struct Surface {
    float3 position;
    float3 normal;
    float3 color;
    float alpha;
    float metallic;
    float smoothness;
    float depthEdgeStrength;
    float normalEdgeStrength;
};

struct BRDF {
    float3 baseDiffuse;
    float3 baseSpecular;
};



float3 GetViewDirection(float3 pos) {
    if (ORTHOGRAPHIC_CAMERA) {
        return normalize(float3(1, 1, 1)); // BAD BAD BAD BAD NO NO NO
    } else {
        return normalize(_WorldSpaceCameraPos - pos);
    }
}

float SimpleDiffuseStrength(float3 normal, float3 lightDir) {
    return saturate(normal, lightDir)
}



/*#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic) {
    const float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BRDF GetBRDF(inout Surface surface, bool applyAlphaToDiffuse = false) {
    BRDF brdf;C
    const float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    if (applyAlphaToDiffuse) {
        brdf.diffuse *= surface.alpha;
    }
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);;
    return brdf;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light) {
    const float3 h = SafeNormalize(light.Direction() + surface.viewDir);
    const float nh2 = Sq(saturate(dot(surface.normal, h)));
    const float lh2 = Sq(saturate(dot(light.Direction(), h)));
    const float r2 = Sq(brdf.roughness);
    const float d2 = Sq(float(nh2 * (r2 - 1.0) + 1.00001)); //todo: check if this is right
    const float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF(Surface surface, BRDF brdf, Light light) {
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

float3 IncomingLight(Surface surface, Light light) {
    return saturate(dot(surface.normal, -light.Direction())) * light.Color();
}

float3 GetLighting(Surface surface, BRDF brdf, Light light) {
    return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}*/

#endif