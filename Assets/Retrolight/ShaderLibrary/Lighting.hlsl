#ifndef RETROLIGHT_LIGHTING_INCLUDED
#define RETROLIGHT_LIGHTING_INCLUDED

#include "Common.hlsl"
#include "Light.hlsl"
#include "Filtering.hlsl"

struct Surface {
    float3 color;
    float alpha;
    float3 normal;

    float metallic;
    float smoothness;
    float depthEdgeStrength;
    float normalEdgeStrength;
};

struct BRDFParams {
    float3 baseDiffuse;
    float3 baseSpecular;
    float3 normal;
    float roughness;

    float3 lightColor;
    float attenuation;

    float3 viewDir;
    float3 lightDir;
};

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(const float metallic) {
    const float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

float PointAttenuation(const float d2, const float r, const float f) {
    const float s2 = saturate(d2 * rcp(Sq(r)));
    return Sq(1 - s2) * rcp(f * s2);
}

float SpotAttenuation(const float3 dirToLight, const float3 lightDir, const float lightCosAngle) {
    const float cosAngleToLight = dot(dirToLight, lightDir);
    return saturate(1.0 - (1.0 - cosAngleToLight) * 1.0/(1.0 - lightCosAngle));
}

BRDFParams GetBRDFParams(
    const Surface surface, const PositionInputs positionInputs,
    Light light, const float2 edges, const bool applyAlphaToDiffuse = false
) {
    BRDFParams params;
    //todo: separate non-light related stuff into separate function to run less times
    const float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    params.baseDiffuse = surface.color * oneMinusReflectivity;
    if (applyAlphaToDiffuse) {
        params.baseDiffuse *= surface.alpha;
    }
    params.baseSpecular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    const float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    params.normal = surface.normal;
    params.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

    params.lightColor = light.Color();
    //todo: this doesn't work for orthographic cameras
    //ideally, this would be a matrix multiplication by the 
    params.viewDir = normalize(-GetCameraRelativePositionWS(positionInputs.positionWS));

    const float edgeStrength =
        edges.x > 0.0 ?
            (1.0 - surface.depthEdgeStrength * edges.x) :
            (1.0 + surface.normalEdgeStrength * edges.y);

    float3 relativeLightPos;
    switch (light.Type()) {
        case DIRECTIONAL_LIGHT:
            params.lightDir = light.Direction();
            params.attenuation = saturate(dot(params.normal, params.lightDir));
            params.attenuation = Quantize(Dither8(params.attenuation, 0.05, positionInputs.positionSS), 8);
            params.attenuation *= edgeStrength;
            break;
        case POINT_LIGHT:
            relativeLightPos = light.position - positionInputs.positionWS;
            params.lightDir = normalize(relativeLightPos);
            params.attenuation = saturate(dot(params.normal, params.lightDir));
            params.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
        
            params.attenuation = Quantize(Dither8(params.attenuation, 0.1, positionInputs.positionSS), 4);
            params.attenuation *= edgeStrength;
            break;
        case SPOT_LIGHT:
            relativeLightPos = light.position - positionInputs.positionWS;
            params.lightDir = normalize(relativeLightPos);
            params.attenuation = saturate(dot(params.normal, params.lightDir));
            params.attenuation *= PointAttenuation(Length2(relativeLightPos), light.Range(), 1);
            params.attenuation *= SpotAttenuation(params.lightDir, light.Direction(), light.CosAngle());
        
            params.attenuation = Quantize(Dither8(params.attenuation, 0.1, positionInputs.positionSS), 4);
            params.attenuation *= edgeStrength;
            break;
        default:
            params.lightDir = float3(0, 1, 0);
            params.attenuation = 0;
            break;
    }

    return params;
}

float3 DirectBRDF(const BRDFParams params) {
    //return surface.normal;
    //return IncomingLight(surface, light);
    const float3 h = SafeNormalize(params.lightDir + params.viewDir);
    const float nh2 = Sq(saturate(dot(params.normal, h)));
    const float lh2 = Sq(saturate(dot(params.lightDir, h)));
    const float r2 = Sq(params.roughness);
    const float d2 = Sq(float(nh2 * (r2 - 1.0) + 1.00001)); //todo: check if this is right
    const float normalization = params.roughness * 4.0 + 2.0;
    const float specularStrength = r2 / (d2 * max(0.1, lh2) * normalization);
    const float3 baseLitColor = specularStrength * params.baseSpecular + params.baseDiffuse;
    return params.lightColor * params.attenuation * baseLitColor;
}

#endif
