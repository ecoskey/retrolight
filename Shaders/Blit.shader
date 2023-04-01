Shader "Hidden/Retrolight/Blit" {
	SubShader {
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		HLSLINCLUDE
		#pragma vertex FullscreenVertex
		#pragma fragment BlitFragment
		#include "../ShaderLibrary/Common.hlsl"
		
		TEXTURE2D(_BlitTexture);
		
		float4 _BlitScaleBias;
		float _BlitMipLevel;
		
		#define FULLSCREEN_ST _BlitScaleBias
		#include "../ShaderLibrary/Fullscreen.hlsl"
		ENDHLSL
		
		Pass {
			Name "BlitPoint"
			
			HLSLPROGRAM
			float4 BlitFragment(V2F input) : SV_Target {
				return SAMPLE_TEXTURE2D_LOD(_BlitTexture, POINT_SAMPLER, input.uv, _BlitMipLevel);
			}
			ENDHLSL
		}
		
		Pass {
			Name "BlitBilinear"
			
			HLSLPROGRAM
			float4 BlitFragment(V2F input) : SV_Target {
				return SAMPLE_TEXTURE2D_LOD(_BlitTexture, BILINEAR_SAMPLER, input.uv, _BlitMipLevel);
			}
			ENDHLSL
		}
	}
}