using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Data {
    public struct ShadowData {
        public readonly TextureHandle DirectionalShadowAtlas;
        public readonly Matrix4x4[] DirectionalShadowMatrices;
        //public readonly TextureHandle OtherShadowAtlas;

        public ShadowData(TextureHandle directionalShadowAtlas, Matrix4x4[] directionalShadowMatrices/*, TextureHandle otherShadowAtlas*/) {
            DirectionalShadowAtlas = directionalShadowAtlas;
            DirectionalShadowMatrices = directionalShadowMatrices;
            //OtherShadowAtlas = otherShadowAtlas;
        }

        public ShadowData ReadAll(RenderGraphBuilder builder) => 
            new ShadowData(builder.ReadTexture(DirectionalShadowAtlas), DirectionalShadowMatrices);

        public ShadowData UseAll(IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Read) => 
            new ShadowData(
                builder.UseTexture(DirectionalShadowAtlas, accessFlags), 
                DirectionalShadowMatrices/*, builder.ReadTexture(OtherShadowAtlas)*/);
    }
}