using UnityEngine;

namespace Retrolight.Runtime {
    public static class Constants {
        public const int MaximumLights = 1024;
        public const int MaxDirectionalShadows = 16;
        public const int MaxOtherShadows = 64;

        public static readonly int
            LightCountId = Shader.PropertyToID("LightCount"),
            LightBufferId = Shader.PropertyToID("Lights"),
            CullingResultsId = Shader.PropertyToID("CullingResults"),
            DirectionalShadowAtlasId = Shader.PropertyToID("DirectionalShadowAtlas"),
            OtherShadowAtlasId = Shader.PropertyToID("OtherShadowAtlas"),
            DirectionalShadowMatricesId = Shader.PropertyToID("DirectionalShadowMatrices"),
            OtherShadowMatricesId = Shader.PropertyToID("OtherShadowMatrices");
    }
}