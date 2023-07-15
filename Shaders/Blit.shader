Shader "Hidden/Retrolight/Blit" {
	SubShader {
		Tags { 
			"RenderType" = "Opaque" 
			"RenderPipeline" = "Retrolight"
		}
		
		ZTest Always
		ZWrite Off
		Cull Off
		
		HLSLINCLUDE
		//probably fine because we don't support VR
		#pragma multi_compile _ BLIT_DECODE_HDR
		#define TEXTURE2D_X TEXTURE2D
		#define SAMPLE_TEXTURE2D_X_LOD SAMPLE_TEXTURE2D_LOD
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
		#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
		ENDHLSL
		
		Pass {
			Name "BlitNearest"
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragNearest
			ENDHLSL
		}
		
		Pass {
			Name "BlitBilinear"
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragBilinear
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadNearest"
			HLSLPROGRAM
			#pragma vertex VertQuad
			#pragma fragment FragNearest
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadBilinear"
			HLSLPROGRAM
			#pragma vertex VertQuad
			#pragma fragment FragBilinear
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingNearest"
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragNearest
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingBilinear"
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragBilinear
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingNearestRepeat"
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragNearestRepeat
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingBilinearRepeat"
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragBilinearRepeat
			ENDHLSL
		}
		
		Pass {
			Name "BlitOctahedralPadding"
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragOctahedralProject //todo: this?
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingNearestMultiply"
			Blend DstColor Zero
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragNearest
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingBilinearMultiply"
			Blend DstColor Zero
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragBilinear
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingNearestMultiplyRepeat"
			Blend DstColor Zero
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragNearestRepeat
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadPaddingBilinearMultiplyRepeat"
			Blend DstColor Zero
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragBilinearRepeat
			ENDHLSL
		}
		
		Pass {
			Name "BlitOctahedralPaddingMultiply"
			Blend DstColor Zero
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragOctahedralProject
			ENDHLSL
		}
		
		Pass {
			Name "BlitCubeToOctahedral2DQuad"
			//todo: ??????????
			HLSLPROGRAM
			#pragma vertex VertQuad
			#pragma fragment FragOctahedralProject
			ENDHLSL
		}
		
		Pass {
			Name "BlitCubeToOctahedral2DQuadPadding"
			//todo: ??????????
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragOctahedralProject
			ENDHLSL
		}
		
		Pass {
			Name "BlitCubeToOctahedral2DQuadSingleChannel"
			//todo: ??????????
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragOctahedralProject
			ENDHLSL
		}
		
		Pass {
			Name "BlitQuadSingleChannel"
			//todo: ??????????
			HLSLPROGRAM
			#pragma vertex VertQuadPadding
			#pragma fragment FragOctahedralProject
			ENDHLSL
		}
	}
}