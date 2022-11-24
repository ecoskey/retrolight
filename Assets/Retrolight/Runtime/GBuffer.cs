using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Runtime {
    public class GBuffer {
        public TextureHandle albedo;
        public TextureHandle depth;
        public TextureHandle normal;

        public GBuffer(TextureHandle albedo, TextureHandle depth, TextureHandle normal) {
            this.albedo = albedo;
            this.depth = depth;
            this.normal = normal;
        }
    }
}