Shader "Hidden/TestBlit" {
	Properties {}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		
		Pass {
			HLSLPROGRAM
			#pragma vertex FullscreenVertex
			#pragma fragment TestFragment
			#include "../ShaderLibrary/Fullscreen.hlsl"

			float4 TestFragment(V2F input) : SV_TARGET {
				return float4(1, 0, 1, 1);
			}
			ENDHLSL
		}
	}
}