using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Data {
    public struct LightingData {
        public readonly TextureHandle FinalColorTex;
        public readonly ComputeBufferHandle CullingResultsBuffer;
        //public readonly TextureHandle DirectionalShadowAtlas/*, OtherShadowAtlas*/;

        public LightingData(
            TextureHandle finalColorTex, ComputeBufferHandle cullingResultsBuffer//,
            // directionalShadowAtlas//, TextureHandle otherShadowAtlas
        ) {
            FinalColorTex = finalColorTex;
            CullingResultsBuffer = cullingResultsBuffer;
            //DirectionalShadowAtlas = directionalShadowAtlas;
            //OtherShadowAtlas = otherShadowAtlas;
        }

        public LightingData ReadAll(RenderGraphBuilder builder) => new LightingData(
            builder.ReadTexture(FinalColorTex), 
            builder.ReadComputeBuffer(CullingResultsBuffer)/*,
            builder.ReadTexture(DirectionalShadowAtlas),
            builder.ReadTexture(OtherShadowAtlas)*/
        );
        
        public LightingData ReadLighting(RenderGraphBuilder builder) => new LightingData(
            builder.ReadTexture(FinalColorTex), 
            builder.ReadComputeBuffer(CullingResultsBuffer)/*,
            DirectionalShadowAtlas,
            OtherShadowAtlas*/
        );
        
        public LightingData ReadShadows(RenderGraphBuilder builder) => new LightingData(
            FinalColorTex,
            CullingResultsBuffer/*,
            builder.ReadTexture(DirectionalShadowAtlas)/*,
            builder.ReadTexture(OtherShadowAtlas)*/
        );
    }
}