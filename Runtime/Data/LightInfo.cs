using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Data {
    public struct LightInfo {
        public readonly int LightCount;
        public readonly ComputeBufferHandle LightsBuffer;

        public LightInfo(int lightCount, ComputeBufferHandle lightsBuffer) {
            LightCount = lightCount;
            LightsBuffer = lightsBuffer;
        }

        public LightInfo ReadAll(RenderGraphBuilder builder) =>
            new LightInfo(LightCount, builder.ReadComputeBuffer(LightsBuffer));
    }
}