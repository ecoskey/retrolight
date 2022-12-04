Shader "Hidden/DumbLitPass" {
	Properties { }
	SubShader {
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off
			
			HLSLPROGRAM
			#pragma vertex LitVertex
			#pragma fragment LitFragment
			#include "../ShaderLibrary/GBuffer.hlsl"
				struct Attributes {
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD;
			};

			struct V2F {
				float4 positionCS : SV_Position;
				float2 uv : TEXCOORD;
			};

			V2F LitVertex(Attributes input) {
				V2F output;
				output.positionCS = TransformObjectToHClip(input.positionOS);
				output.uv = input.uv;
				return output;
			}

			float4 LitFragment(V2F input) : SV_Target {
				float4 albedo = SampleAlbedo(input.uv);
				float3 normal = SampleNormal(input.uv);

				float3 sunDir = normalize(float3(1, 1, 1));
				
				return albedo * saturate(dot(sunDir, normal));
			}
			ENDHLSL
		}
	}
}