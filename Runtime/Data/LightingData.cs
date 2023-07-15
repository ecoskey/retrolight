using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Data {
    public struct LightingData {
        public readonly TextureHandle FinalColorTex;
        public readonly BufferHandle CullingResultsBuffer;
        //public readonly TextureHandle DirectionalShadowAtlas/*, OtherShadowAtlas*/;

        public LightingData(
            TextureHandle finalColorTex, BufferHandle cullingResultsBuffer//,
            // directionalShadowAtlas//, TextureHandle otherShadowAtlas
        ) {
            FinalColorTex = finalColorTex;
            CullingResultsBuffer = cullingResultsBuffer;
            //DirectionalShadowAtlas = directionalShadowAtlas;
            //OtherShadowAtlas = otherShadowAtlas;
        }

        public LightingData ReadAll(RenderGraphBuilder builder) => new LightingData(
            builder.ReadTexture(FinalColorTex), 
            builder.ReadBuffer(CullingResultsBuffer)/*,
            builder.ReadTexture(DirectionalShadowAtlas),
            builder.ReadTexture(OtherShadowAtlas)*/
        );
        
        public LightingData ReadLighting(RenderGraphBuilder builder) => new LightingData(
            builder.ReadTexture(FinalColorTex), 
            builder.ReadBuffer(CullingResultsBuffer)/*,
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