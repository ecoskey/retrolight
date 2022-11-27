Shader "Retrolight/Standard" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		
		[Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1
		
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.0
		_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		
		_DepthEdgeStrength ("Depth Edge Strength", Range(0, 1)) = 0
		_NormalEdgeStrength ("Normal Edge Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }

		Pass {
			Name "GBuffer Pass"
			Tags { "LightMode" = "GBuffer" }
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex GBufferVertex
			#pragma fragment GBufferFragment
			#include "GBufferPass.hlsl"
			ENDHLSL
		}
	}
}