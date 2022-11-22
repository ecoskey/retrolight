#ifndef RETROLIGHT_UNITY_INPUT_INCLUDED
#define RETROLIGHT_UNITY_INPUT_INCLUDED

#include "Common.hlsl"

CBUFFER_START(UnityPerDraw)
    // Space Block
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    float4 unity_LODFade;
    float4 unity_WorldTransformParams;

    // Motion Vector Block
    float4x4 unity_MatrixPreviousM;
    float4x4 unity_MatrixPreviousMI;
    float4 unity_MotionVectorsParams;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

float3 _WorldSpaceCameraPos;

#endif