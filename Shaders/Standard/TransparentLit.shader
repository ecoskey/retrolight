Shader "Retrolight/Standard/TransparentLit" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		
		[Normal] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1
		
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		
		_DepthEdgeStrength ("Edge Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { 
			"RenderPipeline" = "Retrolight"
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		
		ZWrite Off
		BlendOp Add
		Blend One One

		Pass {
			Tags { "LightMode" = "ForwardTransparent" }
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex TransparentVertex
			#pragma fragment TransparentFragment
			#include "TransparentLitInputs.hlsl"
			#include "TransparentLitPass.hlsl"
			ENDHLSL
		}
	}
}