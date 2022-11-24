Shader "Retrolight/TestMRT" {
	Properties {
		[MainColor] _MainColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }

		Pass {
			Name "GBuffer Pass"
			Tags { "LightMode" = "GBuffer" }
			ZWrite On
			
			HLSLPROGRAM
			#pragma vertex TestVertex
			#pragma fragment TestFragment
			#include "../ShaderLibrary/Common.hlsl"

			float4 _Color;

			float4 TestVertex(float4 pos : POSITION) : SV_Position {
				return TransformObjectToHClip(pos);
			}

			float4 TestFragment(float4 pos : SV_Position) : SV_Target0 {
				return _Color * pos;
			}
			ENDHLSL
		}
	}
}