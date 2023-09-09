using UnityEngine.Experimental.Rendering.RenderGraphModule;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Retrolight.Data {
    public readonly struct GBuffer {
        public readonly TextureHandle Diffuse;
        public readonly TextureHandle Specular;
        public readonly TextureHandle Normal;
        //public readonly TextureHandle Depth;

        //public readonly TextureHandle Emission;

        public GBuffer(TextureHandle diffuse,
            TextureHandle specular,
            TextureHandle normal/*,
            TextureHandle depth*/
            /*TextureHandle Emission,*/
        ) {
            Diffuse = diffuse;
            Specular = specular;
            Normal = normal;
            //Depth = depth;
            //Emission = emission;
        }

        public GBuffer ReadAll(RenderGraphBuilder builder) => new GBuffer(
            builder.ReadTexture(Diffuse),
            builder.ReadTexture(Specular),
            builder.ReadTexture(Normal)/*,
            builder.ReadTexture(Depth)*/
        );

        public GBuffer UseAll(IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Read) => new GBuffer(
            builder.UseTexture(Diffuse, accessFlags),
            builder.UseTexture(Specular, accessFlags),
            builder.UseTexture(Normal, accessFlags)/*,
            builder.UseTexture(Depth, accessFlags)*/
            //builder.ReadTexture(Emission),
        );
        
        public GBuffer UseAllFrameBuffer(IRasterRenderGraphBuilder builder, AccessFlags accessFlags) => new GBuffer(
            builder.UseTextureFragment(Diffuse, 0, accessFlags),
            builder.UseTextureFragment(Specular, 1, accessFlags),
            builder.UseTextureFragment(Normal, 2, accessFlags)/*,
            builder.UseTextureFragmentDepth(Depth, accessFlags)*/
            //builder.ReadTexture(Emission),
        );
    }
}