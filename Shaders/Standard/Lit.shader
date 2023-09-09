Shader "Retrolight/Standard/Lit" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		
		[Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1
		
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.0
		
		_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		
		_EdgeStrength ("Edge Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { 
			"RenderPipeline" = "Retrolight"
			"RenderType" = "Opaque" 
			"Queue" = "Geometry" 
		}
		
		HLSLINCLUDE
		#pragma target 3.5
		#pragma multi_compile_instancing
		#pragma shader_feature_local_fragment _Clipping
		#include "LitInputs.hlsl"
		ENDHLSL
		
		Pass {
			Name "Depth Only Pass"
			Tags { "LightMode" = "DepthOnly" }
			
			ZWrite On
			
			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			#include "DepthOnlyPass.hlsl"
			ENDHLSL
		}

		Pass {
			Name "GBuffer Pass"
			Tags { "LightMode" = "GBuffer" }
			
			HLSLPROGRAM
			#pragma vertex GBufferVertex
			#pragma fragment GBufferFragment
			#include "GBufferPass.hlsl"
			ENDHLSL
		}
	}
}