#ifndef RETROLIGHT_COMMON_INCLUDED
#define RETROLIGHT_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_PREV_MATRIX_M unity_MatrixPreviousM
#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_I_VP unity_MatrixInvVP
#define UNITY_MATRIX_P glstate_matrix_projection
#define UNITY_MATRIX_I_P unity_MatrixInvP

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

// Transforms normal from object to world space
float3 TransformObjectToViewNormal(float3 normalOS, bool doNormalize = true) {
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
    return TransformObjectToViewDir(normalOS, doNormalize);
    #else
    // Normal need to be multiply by inverse transpose
    float3 normalVS = mul(normalOS, (float3x3)GetWorldToObjectMatrix() * (float3x3)GetViewToWorldMatrix());
    if (doNormalize)
        return SafeNormalize(normalVS);

    return normalVS;
    #endif
}

#define IS_ORTHOGRAPHIC_CAMERA (unity_OrthoParams.w == 1.0)

#define ACCESS_PROP(prop) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, prop)

uint2 PackFloat3(float3 src) {
    return uint2(
        f32tof16(src.x) | f32tof16(src.y) << 16,
        f32tof16(src.z)
    );
}

uint2 PackFloat4(float4 src) {
    return uint2(
        f32tof16(src.x) | f32tof16(src.y) << 16,
        f32tof16(src.z) | f32tof16(src.w) << 16
    );
}

float3 NormalTangentToWorld(float3 normalTS, float3 normalWS, float4 tangentWS) {
    const float3x3 tangentToWorld = CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
    return TransformTangentToWorld(normalTS, tangentToWorld);
}

#endif
