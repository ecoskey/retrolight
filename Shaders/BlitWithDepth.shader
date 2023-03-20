Shader "Hidden/RetrolightBlitWithDepth" {
	SubShader {
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		Blend One One
		
		Pass {
			HLSLPROGRAM
			#pragma vertex FullscreenVertex
			#pragma fragment BlitFragment
			#include "../ShaderLibrary/Common.hlsl"

			TEXTURE2D(_BlitTexture);
			TEXTURE2D(_InputDepthTexture);

			float4 _BlitScaleBias;
			float _BlitMipLevel;

			#define FULLSCREEN_ST _BlitScaleBias
			#include "../ShaderLibrary/Fullscreen.hlsl"
			
			struct BlitOut {
				float4 color : SV_Target;
				float depth : SV_Depth;
			};
			
			BlitOut BlitFragment(V2F input) {
				BlitOut output;
				output.color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, DEFAULT_SAMPLER, input.uv, _BlitMipLevel);
				output.depth = SAMPLE_TEXTURE2D_LOD(_InputDepthTexture, DEFAULT_SAMPLER, input.uv, _BlitMipLevel).r;
				return output;
			}
			ENDHLSL
		}
	}
}