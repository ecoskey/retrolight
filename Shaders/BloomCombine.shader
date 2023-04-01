Shader "Hidden/Retrolight/BloomCombine" {
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
		#include "../ShaderLibrary/Fullscreen.hlsl"
		#pragma vertex FullscreenVertex
		#pragma fragment BloomCombineFragment

		TEXTURE2D(_Source1);
		TEXTURE2D(_Source2);
		ENDHLSL
		
		Pass {
			Name "BloomCombine"

			HLSLPROGRAM
			float4 BloomCombineFragment(V2F input) : SV_Target {
				float3 lowRes = SAMPLE_TEXTURE2D_LOD(_Source1, BILINEAR_SAMPLER, input.uv, 0).rgb;
				float3 highRes = SAMPLE_TEXTURE2D_LOD(_Source2, BILINEAR_SAMPLER, input.uv, 0).rgb;
				return float4(lowRes + highRes, 1);
				LOAD_TEXTURE2D_LOD()
			}
			ENDHLSL
		}
		
		/*Pass {
			Name "BloomCombineBicubic"

			HLSLPROGRAM
			float4 BloomCombineFragment(V2F input) : SV_Target {
				 return BLUR_Y(TEXTURE2D_ARGS(_BlurSource, BILINEAR_SAMPLER), input.uv, _TexelSize);
			}
			ENDHLSL
		}*/
	}
}