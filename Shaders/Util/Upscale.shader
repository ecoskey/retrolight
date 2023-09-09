Shader "Hidden/Retrolight/Util/Upscale" {
	SubShader {
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		
		Pass {
			Name "Upscale"
			
			HLSLPROGRAM
			//#include "Packages/net.cosc.retrolight/ShaderLibrary/Common.hlsl"
			#include "Packages/net.cosc.retrolight/ShaderLibrary/Fullscreen.hlsl"
			#include "Packages/net.cosc.retrolight/ShaderLibrary/Sampling.hlsl"
			
			#pragma vertex FullscreenVertex
			#pragma fragment UpscaleFragment

			TEXTURE2D(SrcTex0);
			float4 SrcTex0_ST;
			float4 SrcTex0Res;

			float4 UpscaleFragment(V2F input) : SV_Target {
				//return float4(1, 1, 0, 1);
				//return float4(frac(input.uv * SrcTex0Res.xy / 10.0), 0, 1);
				return SampleTex2DSprite(SrcTex0, input.uv * SrcTex0_ST.xy + SrcTex0_ST.zw, SrcTex0Res);
			}
			ENDHLSL
		}
	}
}