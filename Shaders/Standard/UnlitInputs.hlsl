#ifndef RETROLIGHT_TRANSPARENT_LIT_INPUTS_INCLUDED
#define RETROLIGHT_TRANSPARENT_LIT_INPUTS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _MainColor ACCESS_PROP(_MainColor)
#define _MainTex_ST ACCESS_PROP(_MainTex_ST)

#endif