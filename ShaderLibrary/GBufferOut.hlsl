#ifndef RETROLIGHT_GBUFFER_OUT_INCLUDED
#define RETROLIGHT_GBUFFER_OUT_INCLUDED

#include "Lighting.hlsl"

struct GBufferOut {
    float4 diffuse_roughness : SV_Target0;
    float4 specular_edges : SV_Target1;
    float4 normal_alpha : SV_Target2;
};

GBufferOut GetGBufferOut(Surface surface) {
    GBufferOut output;

    output.diffuse_roughness = float4(surface.baseDiffuse, surface.roughness);
    output.specular_edges = float4(surface.baseSpecular, surface.edgeStrength);
    output.normal_alpha = float4((surface.normal + 1) / 2, surface.alpha);
    return output;
}

GBufferOut GetGBufferOut(
    float3 diffuse, float3 specular, float3 normal,
    float alpha, float roughness, float edgeStrength
) {
    GBufferOut output;

    output.diffuse_roughness = float4(diffuse, roughness);
    output.specular_edges = float4(specular, edgeStrength);
    output.normal_alpha = float4((normal + 1) / 2, alpha);
    return output;
}



#endif