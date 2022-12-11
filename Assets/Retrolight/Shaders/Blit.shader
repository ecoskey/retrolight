Shader "Hidden/RetrolightBlit"
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

			float4 _BlitScaleBias;
			float _BlitMipLevel;
			
			struct VertexInput {
			    uint vertexId : SV_VertexID;
			};

			struct V2F {
			    float4 positionCS : SV_Position;
			    float2 uv : V2F_UV;
			};

			V2F BlitVertex(VertexInput input) {
				V2F output;
				output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexId);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexId) * _BlitScaleBias.xy + _BlitScaleBias.zw;
				return output;
			}

			float4 BlitFragment(V2F input) : SV_Target {
				return SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_PointClamp, input.uv, _BlitMipLevel);
			}
			
			ENDHLSL
		}
	}
}