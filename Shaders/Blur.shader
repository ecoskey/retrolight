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
			#define BLUR_X gaussianBlur9_X
			#define BLUR_Y gaussianBlur9_Y
		#elif BOX_3
			#define BLUR_X boxBlur3_X
			#define BLUR_Y boxBlur3_Y
		#elif BOX_5
			#define BLUR_X boxBlur5_X
			#define BLUR_Y boxBlur5_Y
		#elif BOX_7
			#define BLUR_X boxBlur7_X
			#define BLUR_Y boxBlur7_Y
		#else
			#define BLUR_X boxBlur3_X
			#define BLUR_Y boxBlur3_Y
		#endif

		TEXTURE2D(_BlurSource);
		float _TexelSize;
		
		ENDHLSL
		
		Pass {
			Name "BlurX"

			HLSLPROGRAM
			float4 BlurFragment(V2F input) : SV_Target {
				return BLUR_X(TEXTURE2D_ARGS(_BlurSource, BILINEAR_SAMPLER), input.uv, 0, _TexelSize);
			}
			ENDHLSL
		}
		
		Pass {
			Name "BlurY"

			HLSLPROGRAM
			float4 BlurFragment(V2F input) : SV_Target {
				 return BLUR_Y(TEXTURE2D_ARGS(_BlurSource, BILINEAR_SAMPLER), input.uv, 0, _TexelSize);
			}
			ENDHLSL
		}
	}
}