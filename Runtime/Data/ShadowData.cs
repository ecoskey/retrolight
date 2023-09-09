using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Retrolight.Data {
    public readonly struct ShadowData {
        public readonly TextureHandle DirectionalShadowAtlas;
        public readonly Matrix4x4[] DirectionalShadowMatrices;
        
        //public readonly TextureHandle OtherShadowAtlas;

        private ShadowData(TextureHandle directionalShadowAtlas, Matrix4x4[] directionalShadowMatrices/*, 
            TextureHandle otherShadowAtlas*/
        ) {
            DirectionalShadowAtlas = directionalShadowAtlas;
            DirectionalShadowMatrices = directionalShadowMatrices;
            //OtherShadowAtlas = otherShadowAtlas;
        }

        public static ShadowData Create(TextureHandle directionalShadowAtlas, Matrix4x4[] directionalShadowMatrices) =>
            new ShadowData(directionalShadowAtlas, directionalShadowMatrices);

        
        public ShadowData ReadAll(RenderGraphBuilder builder) {
            return Create(builder.ReadTexture(DirectionalShadowAtlas), DirectionalShadowMatrices);
        }


        public ShadowData UseAll(IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Read) {
            return Create(builder.UseTexture(DirectionalShadowAtlas, accessFlags), DirectionalShadowMatrices);
        }
    }
}