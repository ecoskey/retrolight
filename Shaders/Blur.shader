Shader "Hidden/Retrolight/Blur" {
	SubShader {
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "../ShaderLibrary/Filtering.hlsl"
		#include "../ShaderLibrary/Fullscreen.hlsl"
		#pragma vertex FullscreenVertex
		#pragma fragment BlurFragment
		
		#pragma multi_compile GAUSSIAN_9 BOX_3 BOX_5 BOX_7
		#if GAUSSIAN_9
			#define BLUR_X gaussian9_X
			#define BLUR_Y gaussian9_Y
		#elif BOX_3
			#define BLUR_X box3_X
			#define BLUR_Y box3_Y
		#elif BOX_5
			#define BLUR_X box5_X
			#define BLUR_Y box5_Y
		#elif BOX_7
			#define BLUR_X box7_X
			#define BLUR_Y box7_Y
		#else
			#define BLUR_X box3_X
			#define BLUR_Y box3_Y
		#endif

		TEXTURE2D(_BlurSource);
		float2 _TexelSize;
		
		ENDHLSL
		
		Pass {
			Name "BlurX"

			HLSLPROGRAM
			float4 BlurFragment(V2F input) : SV_Target {
				return CustomBox13(TEXTURE2D_ARGS(_BlurSource, BILINEAR_SAMPLER), input.uv, 0, _TexelSize);
			}
			ENDHLSL
		}
		
		Pass {
			Name "BlurY"

			HLSLPROGRAM
			float4 BlurFragment(V2F input) : SV_Target {
				 return CustomBox13(TEXTURE2D_ARGS(_BlurSource, BILINEAR_SAMPLER), input.uv, 0, _TexelSize);
			}
			ENDHLSL
		}
	}
}