using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Runtime {
    public class GBuffer {
        public readonly TextureHandle albedo;
        public readonly TextureHandle depth;
        public readonly TextureHandle normal;
        //public readonly TextureHandle emission;
        public readonly TextureHandle attributes;

        public GBuffer(
            TextureHandle albedo, TextureHandle depth, TextureHandle normal,
            /*TextureHandle emission,*/ TextureHandle attributes
        ) {
            this.albedo = albedo;
            this.depth = depth;
            this.normal = normal;
            //this.emission = emission;
            this.attributes = attributes;
        }

        public GBuffer ReadAll(RenderGraphBuilder builder) => new GBuffer(
            builder.ReadTexture(albedo),
            builder.ReadTexture(depth),
            builder.ReadTexture(normal),
            //builder.ReadTexture(emission),
            builder.ReadTexture(attributes)
        );
    }
}