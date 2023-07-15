using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Data {
    public readonly struct LightInfo {
        public readonly int LightCount;
        public readonly BufferHandle LightsBuffer;

        public LightInfo(int lightCount, BufferHandle lightsBuffer) {
            LightCount = lightCount;
            LightsBuffer = lightsBuffer;
        }

        public LightInfo ReadAll(RenderGraphBuilder builder) =>
            new LightInfo(LightCount, builder.ReadBuffer(LightsBuffer));
    }
}