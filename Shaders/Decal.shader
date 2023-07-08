Shader "Retrolight/Decal" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (1, 1, 1, 1)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		
		//[Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		//_NormalScale("Normal Scale", Range(0, 1)) = 1
		
		//_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.0
		
		//_Metallic ("Metallic", Range(0, 1)) = 0.0
		//_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		
		//_DepthEdgeStrength ("Depth Edge Strength", Range(0, 1)) = 0
		//_NormalEdgeStrength ("Normal Edge Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { 
			"RenderPipeline" = "Retrolight"
			"RenderType" = "TransparenT" 
			"Queue" = "Transparent" 
		}

		Pass {
			Name "GBufferPass"
			Tags { "LightMode" = "RetrolightDecal" }
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex DecalVertex
			#pragma fragment DecalFragment
			#include "DecalPass.hlsl"
			ENDHLSL
		}
	}
}