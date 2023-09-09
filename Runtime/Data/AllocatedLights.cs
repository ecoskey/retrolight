using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using AccessFlags = UnityEngine.Experimental.Rendering.RenderGraphModule.IBaseRenderGraphBuilder.AccessFlags;

namespace Retrolight.Data {
    public readonly struct AllocatedLights {
        public readonly int LightCount;
        public readonly BufferHandle LightsBuffer;

        //public readonly List<ShadowedLight> ShadowedDirectionalLights;

        //public readonly List<int> ShadowedOtherLights;
        
        /*public readonly TextureHandle OtherShadowAtlas;
        public readonly int[] ShadowedOtherLights;*/

        public AllocatedLights(
            int lightCount, BufferHandle lightsBuffer//, List<ShadowedLight> shadowedDirectionalLights
        ) {
            LightCount = lightCount;
            LightsBuffer = lightsBuffer;
            //ShadowedDirectionalLights = shadowedDirectionalLights;
        }

        public AllocatedLights ReadAll(RenderGraphBuilder builder) => 
            new AllocatedLights(LightCount, builder.ReadBuffer(LightsBuffer)/*, ShadowedDirectionalLights*/);

        public AllocatedLights UseAll(IBaseRenderGraphBuilder builder, AccessFlags accessFlags = AccessFlags.Read) =>
            new AllocatedLights(LightCount, builder.UseBuffer(LightsBuffer, accessFlags)/*, ShadowedDirectionalLights*/);
    }
}