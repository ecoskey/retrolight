#ifndef RETROLIGHT_UNITY_INPUT_INCLUDED
#define RETROLIGHT_UNITY_INPUT_INCLUDED

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

CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixV;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixInvV;
float4x4 unity_MatrixInvVP;
float4x4 glstate_matrix_projection;
float4x4 unity_MatrixInvP;
CBUFFER_END

CBUFFER_START(UnityPerCamera)
float3 _WorldSpaceCameraPos;
float4 _ProjectionParams;
float4 _ZBufferParams;
float4 unity_OrthoParams;
CBUFFER_END

#endif