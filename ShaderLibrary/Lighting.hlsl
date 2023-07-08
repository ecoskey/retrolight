#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#include "Common.hlsl"
#include "Light.hlsl"
#include "Filtering.hlsl"
#include "Culling.hlsl"
#include "Shadows.hlsl"


ByteAddressBuffer LightCullingResults;

float3 GetViewDir(float3 positionWS) {
    return ORTHOGRAPHIC_CAMERA ?
        UNITY_MATRIX_I_V[2].xyz :
        normalize(-GetCameraRelativePositionWS(positionWS));
}

// SURFACE

struct Surface {
    float3 baseDiffuse;
    float alpha;
    float3 baseSpecular;
    float3 normal;
    float roughness;
    float edgeStrength;
};

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(const float metallic) {
    const float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

Surface GetMetallicSurface(
    const float3 color, const float alpha, const float3 normal,
    const float metallic, const float smoothness, const float edgeStrength, const bool applyAlphaToDiffuse = false
) {
    Surface surface;
    const float oneMinusReflectivity = OneMinusReflectivity(metallic);
    surface.baseDiffuse = color * oneMinusReflectivity;
    if (applyAlphaToDiffuse) {
        surface.baseDiffuse *= alpha;
    }
    surface.alpha = alpha;
    surface.baseSpecular = lerp(MIN_REFLECTIVITY, color, metallic);
    const float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    surface.normal = normal;
    surface.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    surface.edgeStrength = edgeStrength;
    return surface;
}

// LIGHTING

struct LightingData {
    float3 color;
    float attenuation;
    float3 lightDir;
};

float PointAttenuation(const float d2, const float r, const float f) {
    const float s2 = saturate(d2 * rcp(Sq(r)));
    return Sq(1 - s2) * rcp(1 + f * s2);
}

float SpotAttenuation(const float3 dirToLight, const float3 lightDir, const float lightCosAngle) {
    const float cosAngleToLight = dot(dirToLight, lightDir);
    return saturate(1.0 - (1.0 - cosAngleToLight) * 1.0/(1.0 - lightCosAngle));
}

LightingData GetLighting(Light light, const float3 surfaceNormal, const PositionInputs positionInputs) {
    LightingData lighting;

    lighting.color = light.Color();
    const uint2 noiseCoords = positionInputs.positionSS;
    float3 relativeLightPos;
    
    switch (light.Type()) {
        case DIRECTIONAL_LIGHT:
            lighting.lightDir = light.Direction();
            lighting.attenuation = saturate(dot(surfaceNormal, lighting.lightDir));
            lighting.attenuation = Quantize(Dither8(lighting.attenuation, 0.05, noiseCoords), 8);
            //if (light.Flags() & F_LIGHT_SHADOWED)
                //params.attenuation *= GetDirectionalShadowAttenuation(positionInputs.positionWS, light.ShadowStrength());
            break;
        case POINT_LIGHT:
            relativeLightPos = light.position - positionInputs.positionWS;
            lighting.lightDir = normalize(relativeLightPos);
            lighting.attenuation = saturate(dot(surfaceNormal, lighting.lightDir));
            lighting.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
            lighting.attenuation = Quantize(Dither8(lighting.attenuation, 0.1, noiseCoords), 6);
            break;
        case SPOT_LIGHT:
            relativeLightPos = light.position - positionInputs.positionWS;
            lighting.lightDir = normalize(relativeLightPos);
            lighting.attenuation = saturate(dot(surfaceNormal, lighting.lightDir));
            //lighting.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
            lighting.attenuation *= SpotAttenuation(lighting.lightDir, light.Direction(), light.CosAngle());
            lighting.attenuation = Quantize(Dither8(lighting.attenuation, 0.1, noiseCoords), 6);
            break;
        default:
            lighting.lightDir = float3(0, 1, 0);
            lighting.attenuation = 0;
            break;
    }

    return lighting;
}

#define BRDF_PARAMS Surface surface, LightingData lighting, float3 viewDir

float3 DirectBRDF(BRDF_PARAMS) {
    //return surface.normal;
    //return IncomingLight(surface, light);
    const float3 h = SafeNormalize(lighting.lightDir + viewDir);
    const float nh2 = Sq(saturate(dot(surface.normal, h)));
    const float lh2 = Sq(saturate(dot(lighting.lightDir, h)));
    const float r2 = Sq(surface.roughness);
    const float d2 = Sq(float(nh2 * (r2 - 1.0) + 1.00001)); //todo: check if this is right
    const float normalization = surface.roughness * 4.0 + 2.0;
    const float specularStrength = r2 / (d2 * max(0.1, lh2) * normalization);
    const float3 baseLitColor = specularStrength * surface.baseSpecular + surface.baseDiffuse;
    return lighting.color * lighting.attenuation * baseLitColor;
}

float3 CartoonBRDF(BRDF_PARAMS) {
    const float3 h = SafeNormalize(lighting.lightDir + viewDir);
    float specular  = normalize(dot(surface.normal, h));
    const float steps = max(surface.roughness * 2, 0.01);
    specular = round(specular * steps) / steps;
    const float3 baseLitColor = specular * surface.baseSpecular + surface.baseDiffuse;
    return lighting.color * lighting.attenuation * baseLitColor;
}

#endif
