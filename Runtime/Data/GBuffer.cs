using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Data {
    public readonly struct GBuffer {
        public readonly TextureHandle Diffuse;
        public readonly TextureHandle Specular;
        public readonly TextureHandle Depth;
        public readonly TextureHandle Normal;

        //public readonly TextureHandle Emission;

        public GBuffer(
            TextureHandle diffuse, TextureHandle specular, TextureHandle depth, TextureHandle normal
            /*TextureHandle Emission,*/ 
        ) {
            Diffuse = diffuse;
            Specular = specular;
            Depth = depth;
            Normal = normal;
            //Emission = emission;
        }

        public GBuffer ReadAll(RenderGraphBuilder builder) => new GBuffer(
            builder.ReadTexture(Diffuse),
            builder.ReadTexture(Specular),
            builder.ReadTexture(Depth),
            builder.ReadTexture(Normal)
            //builder.ReadTexture(Emission),
        );
    }
}