Shader "Retrolight/Standard/Unlit" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
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
			#pragma vertex UnlitVertex
			#pragma fragment UnlitFragment
			#include "UnlitInputs.hlsl"
			#include "UnlitPass.hlsl"
			ENDHLSL
		}
	}
}