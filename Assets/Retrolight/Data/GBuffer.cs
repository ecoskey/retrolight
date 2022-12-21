using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Runtime {
    public readonly struct GBuffer {
        public readonly TextureHandle Albedo;
        public readonly TextureHandle Depth;

        public readonly TextureHandle Normal;

        //public readonly TextureHandle Emission;
        public readonly TextureHandle Attributes;

        public GBuffer(
            TextureHandle albedo, TextureHandle depth, TextureHandle normal,
            /*TextureHandle Emission,*/ TextureHandle attributes
        ) {
            Albedo = albedo;
            Depth = depth;
            Normal = normal;
            //Emission = emission;
            Attributes = attributes;
        }

        public GBuffer ReadAll(RenderGraphBuilder builder) => new GBuffer(
            builder.ReadTexture(Albedo),
            builder.ReadTexture(Depth),
            builder.ReadTexture(Normal),
            //builder.ReadTexture(Emission),
            builder.ReadTexture(Attributes)
        );
    }
}