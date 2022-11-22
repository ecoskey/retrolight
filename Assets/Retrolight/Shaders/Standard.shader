Shader "Retrolight/Standard" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		
		[Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1
		
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.0
		_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		
		[Toggle(_ENABLE_EDGES)] _EnableEdges ("Use Edges", Float) = 0
		_DepthEdgeStrength ("Depth Edge Strength", Range(0, 1)) = 0
		_NormalEdgeStrength ("Normal Edge Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }

		Pass {
			Tags { "LightMode" = "GBuffer" }
			ZWrite On
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex GBufferVertex
			#pragma fragment GBufferFragment
			#include "GBufferPass.hlsl"
			ENDHLSL
		}
		
		Pass {
			Tags { "LightMode" = "Edges" }
			ZWrite Off
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma shader_feature _EDGES_ENABLED
			#pragma vertex EdgesVertex
			#pragma fragment EdgesFragment
			#include "EdgesPass.hlsl"
			ENDHLSL
		}
	}
}