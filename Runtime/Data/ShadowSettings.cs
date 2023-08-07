using System;
using UnityEngine;

namespace Data {
    [Serializable]
    public struct ShadowSettings {
        public bool enableShadows;

        [Range(1, 4)] public byte directionalCascades;
        [Range(1, 4)] public byte maxDirectionalShadows;
        public ShadowmapSize directionalShadowmapSize;

        [Range(1, 64)] public byte maxOtherShadows;
        public ShadowmapSize otherShadowmapSize;

        public ShadowSettings Validate() {
            return new ShadowSettings {
                enableShadows = enableShadows,
                directionalCascades = directionalCascades,
                maxDirectionalShadows = maxDirectionalShadows,
                directionalShadowmapSize =
                    (ShadowmapSize) Math.Min(
                        (int) directionalShadowmapSize,
                        (int) ShadowmapSize._1024
                    ),
                otherShadowmapSize =
                    (ShadowmapSize) Math.Min(
                        (int) directionalShadowmapSize,
                        (int) ShadowmapSize._512
                    )
            };
        }
    }
}