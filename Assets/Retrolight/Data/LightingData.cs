using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Data {
    public struct LightingData {
        public readonly ComputeBufferHandle LightBuffer, CullingResultsBuffer;
        public readonly TextureHandle DirectionalShadowAtlas/*, OtherShadowAtlas*/;

        public LightingData(
            ComputeBufferHandle lightBuffer, ComputeBufferHandle cullingResultsBuffer,
            TextureHandle directionalShadowAtlas//, TextureHandle otherShadowAtlas
        ) {
            LightBuffer = lightBuffer;
            CullingResultsBuffer = cullingResultsBuffer;
            DirectionalShadowAtlas = directionalShadowAtlas;
            //OtherShadowAtlas = otherShadowAtlas;
        }

        public LightingData ReadAll(RenderGraphBuilder builder) => new LightingData(
            builder.ReadComputeBuffer(LightBuffer), 
            builder.ReadComputeBuffer(CullingResultsBuffer),
            builder.ReadTexture(DirectionalShadowAtlas)/*,
            builder.ReadTexture(OtherShadowAtlas)*/
        );
        
        public LightingData ReadLighting(RenderGraphBuilder builder) => new LightingData(
            builder.ReadComputeBuffer(LightBuffer), 
            builder.ReadComputeBuffer(CullingResultsBuffer),
            DirectionalShadowAtlas/*,
            OtherShadowAtlas*/
        );
        
        public LightingData ReadShadows(RenderGraphBuilder builder) => new LightingData(
            LightBuffer,
            CullingResultsBuffer,
            builder.ReadTexture(DirectionalShadowAtlas)/*,
            builder.ReadTexture(OtherShadowAtlas)*/
        );
    }
}