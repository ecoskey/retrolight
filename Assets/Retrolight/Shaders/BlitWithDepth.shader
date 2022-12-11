Shader "Hidden/RetrolightBlitWithDepth"
{
	SubShader
	{
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		
		Pass
		{
			HLSLPROGRAM
			#pragma vertex BlitVertex
			#pragma fragment BlitFragment
			#include "../ShaderLibrary/Common.hlsl"

			TEXTURE2D(_BlitTexture);
			SAMPLER(sampler_PointClamp);

			TEXTURE2D(_InputDepthTexture);

			float4 _BlitScaleBias;
			float _BlitMipLevel;
			
			struct VertexInput {
			    uint vertexId : SV_VertexID;
			};

			struct V2F {
			    float4 positionCS : SV_Position;
			    float2 uv : V2F_UV;
			};

			struct BlitOut {
				float4 color : SV_Target;
				float depth : SV_Depth;
			};

			V2F BlitVertex(VertexInput input) {
				V2F output;
				output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexId);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexId) * _BlitScaleBias.xy + _BlitScaleBias.zw;
				return output;
			}

			BlitOut BlitFragment(V2F input) {
				BlitOut output;
				output.color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_PointClamp, input.uv, _BlitMipLevel);
				output.depth = SAMPLE_TEXTURE2D_LOD(_InputDepthTexture, sampler_PointClamp, input.uv, _BlitMipLevel);
				return output;
			}
			
			ENDHLSL
		}
	}
}