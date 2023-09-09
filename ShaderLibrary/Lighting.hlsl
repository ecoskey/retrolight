#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#include "Common.hlsl"
#include "Light.hlsl"
#include "Filtering.hlsl"
#include "Shadows.hlsl"
#include "UnityInput.hlsl"

float3 GetViewDir(float3 positionWS) {
    return IS_ORTHOGRAPHIC_CAMERA ?
        UNITY_MATRIX_I_V[2].xyz :
        SafeNormalize(_WorldSpaceCameraPos - GetAbsolutePositionWS(positionWS));
}

float3 GetViewDirVS(float3 positionVS) {
    return IS_ORTHOGRAPHIC_CAMERA ? float3(0, 0, 1): normalize(-positionVS);
}

// SURFACE

struct Surface {
    float3 baseDiffuse;
    float alpha;
    float3 baseSpecular;
    float3 normalVS;
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
    surface.normalVS = normal;
    const float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    surface.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    surface.edgeStrength = edgeStrength;
    return surface;
}

// LIGHTING

struct LightingData {
    float3 color;
    float attenuation;
    float3 dirVS;
};

float PointAttenuation(const float d2, const float r, const float f) {
    const float s2 = saturate(d2 * rcp(Sq(r)));
    return Sq(1 - s2) * rcp(1 + f * s2);
}

float SpotAttenuation(const float3 dirToLight, const float3 lightDir, const float lightCosAngle) {
    const float cosAngleToLight = dot(dirToLight, lightDir);
    return saturate(1.0 - (1.0 - cosAngleToLight) * 1.0/(1.0 - lightCosAngle));
}

LightingData GetLighting(Light light, const float3 normal, const float3 positionVS, const float snormNoise, const float noiseStrength) {
    LightingData lighting;

    lighting.color = light.Color();
    float3 relativeLightPos;
    
    switch (light.Type()) {
        case DIRECTIONAL_LIGHT:
            lighting.dirVS = light.Direction();
            lighting.attenuation = saturate(dot(normal, lighting.dirVS));
            //dither with some transfer function? currently dithering is different for different surface angles
            lighting.attenuation = Filtering::Quantize(lighting.attenuation + snormNoise * noiseStrength, 8, noiseStrength);

            if (HasFlag(light.Flags(), F_LIGHT_SHADOWED))
                lighting.attenuation *= GetDirectionalShadowAttenuation(
                    light.ShadowIndex(), positionVS,
                    light.ShadowStrength()
                );
            break;
        case POINT_LIGHT:
            relativeLightPos = light.positionVS - positionVS;
            lighting.dirVS = normalize(relativeLightPos);
            lighting.attenuation = saturate(dot(normal, lighting.dirVS));
            lighting.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
            lighting.attenuation = Filtering::Quantize(lighting.attenuation + snormNoise * noiseStrength, 6, noiseStrength);
            break;
        case SPOT_LIGHT:
            relativeLightPos = light.positionVS - positionVS;
            lighting.dirVS = normalize(relativeLightPos);
            lighting.attenuation = saturate(dot(normal, lighting.dirVS));
            lighting.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
            lighting.attenuation *= SpotAttenuation(lighting.dirVS, light.Direction(), light.CosAngle());
            lighting.attenuation = Filtering::Quantize(lighting.attenuation + snormNoise * noiseStrength, 6, noiseStrength);
            break;
        default:
            lighting.dirVS = float3(0, 0, -1);
            lighting.attenuation = 0;
            break;
    }

    return lighting;
}

float3 DirectBRDF(Surface surface, LightingData lighting, float3 viewDirVS) {
    //return surface.normal;
    //return IncomingLight(surface, light);
    const float3 h = SafeNormalize(lighting.dirVS + viewDirVS);
    const float nh2 = Sq(saturate(dot(surface.normalVS, h)));
    const float lh2 = Sq(saturate(dot(lighting.dirVS, h)));
    const float r2 = Sq(surface.roughness);
    const float d2 = Sq(float(nh2 * (r2 - 1.0) + 1.00001)); //todo: check if this is right
    const float normalization = surface.roughness * 4.0 + 2.0;
    const float specularStrength = r2 / (d2 * max(0.1, lh2) * normalization);
    const float3 baseLitColor = specularStrength * surface.baseSpecular + surface.baseDiffuse;
    return lighting.color * lighting.attenuation * baseLitColor;
}

float3 CartoonBRDF(Surface surface, LightingData lighting, float3 viewDir) {
    const float3 h = SafeNormalize(lighting.dirVS + viewDir);
    float specular  = normalize(dot(surface.normalVS, h));
    const float steps = max(surface.roughness * 2, 0.01);
    specular = round(specular * steps) / steps;
    const float3 baseLitColor = specular * surface.baseSpecular + surface.baseDiffuse;
    return lighting.color * lighting.attenuation * baseLitColor;
}

#endif
