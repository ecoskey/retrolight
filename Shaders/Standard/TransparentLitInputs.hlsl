#ifndef RETROLIGHT_TRANSPARENT_LIT_INPUTS_INCLUDED
#define RETROLIGHT_TRANSPARENT_LIT_INPUTS_INCLUDED

#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(float, _NormalScale)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
    UNITY_DEFINE_INSTANCED_PROP(float, _EdgeStrength)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _MainColor ACCESS_PROP(_MainColor)
#define _MainTex_ST ACCESS_PROP(_MainTex_ST)
#define _NormalScale ACCESS_PROP(_NormalScale)
#define _Metallic ACCESS_PROP(_Metallic)
#define _Smoothness ACCESS_PROP(_Smoothness)
#define _EdgeStrength ACCESS_PROP(_EdgeStrength)

#endif