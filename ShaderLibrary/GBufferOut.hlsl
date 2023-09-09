#ifndef RETROLIGHT_GBUFFER_OUT_INCLUDED
#define RETROLIGHT_GBUFFER_OUT_INCLUDED

#include "Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

struct GBufferOut {
    unorm float4 diffuse_roughness : SV_Target0;
    unorm float4 specular_edges : SV_Target1;
    snorm float2 normal : SV_Target2;
};

GBufferOut GetGBufferOut(Surface surface) {
    GBufferOut output;
    output.diffuse_roughness = float4(surface.baseDiffuse, surface.roughness);
    output.specular_edges = float4(surface.baseSpecular, surface.edgeStrength);
    output.normal = PackNormalOctQuadEncode(surface.normalVS);
    return output;
}

GBufferOut GetGBufferOut(float3 diffuse, float3 specular, float3 normal, float roughness, float edgeStrength) {
    GBufferOut output;
    output.diffuse_roughness = float4(diffuse, roughness);
    output.specular_edges = float4(specular, edgeStrength);
    output.normal = PackNormalOctQuadEncode(normal);
    return output;
}



#endif