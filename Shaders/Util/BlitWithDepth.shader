Shader "Hidden/Retrolight/Util/BlitWithDepth" {
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
		#include "..\..\ShaderLibrary\Sampling.hlsl"
		#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"

		TEXTURE2D(_BlitTexture);
		TEXTURE2D(_InputDepthTexture);

		float4 _BlitScaleBias;
		float _BlitMipLevel;

		#define FULLSCREEN_ST _BlitScaleBias
		#include "Packages/net.cosc.retrolight/ShaderLibrary/Fullscreen.hlsl"
		
		struct BlitOut {
			float4 color : SV_Target;
			float depth : SV_Depth;
		};
		ENDHLSL
		
		Pass {
			Name "BlitWithDepthPoint"
			
			HLSLPROGRAM
			BlitOut BlitFragment(V2F input) {
				BlitOut output;
				output.color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, POINT_SAMPLER, input.uv, _BlitMipLevel);
				output.depth = SAMPLE_TEXTURE2D_LOD(_InputDepthTexture, POINT_SAMPLER, input.uv, _BlitMipLevel).r;
				return output;
			}
			ENDHLSL
		}
		
		Pass {
			Name "BlitWithDepthBilinear"
			
			HLSLPROGRAM
			BlitOut BlitFragment(V2F input) {
				BlitOut output;
				output.color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, BILINEAR_SAMPLER, input.uv, _BlitMipLevel);
				output.depth = SAMPLE_TEXTURE2D_LOD(_InputDepthTexture, BILINEAR_SAMPLER, input.uv, _BlitMipLevel).r;
				return output;
			}
			ENDHLSL
		}
	}
}