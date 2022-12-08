using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using LightType = UnityEngine.LightType;

namespace Retrolight.Runtime {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        private Vector3 position;
        private uint type16_range16;
        private Vector2Int color48_extra16;
        private Vector2Int direction;

        public const int Stride = sizeof(float) * 3 + sizeof(uint) + sizeof(int) * 2 + sizeof(int) * 2;
        
        private enum PackedLightType {
            Directional = 0,
            Point = 1,
            Spot = 2,
            Line = 3
        }

        public PackedLight(VisibleLight light) {
            switch (light.lightType) {
                case LightType.Directional:
                    position = Vector3.zero;
                    type16_range16 = (uint) ((uint) PackedLightType.Directional | Mathf.FloatToHalf(light.range) << 16);
                    color48_extra16 = new Vector2Int( //todo: possibly incorrectly casting to int?
                        Mathf.FloatToHalf(light.finalColor.r) | Mathf.FloatToHalf(light.finalColor.g) << 16,
                        Mathf.FloatToHalf(light.finalColor.b)
                    );
                    direction = new Vector2Int(
                        Mathf.FloatToHalf(light.finalColor.r) | Mathf.FloatToHalf(light.finalColor.g) << 16,
                        Mathf.FloatToHalf(light.finalColor.b)
                    );
                    break;
                default: 
                    position = Vector3.zero;
                    type16_range16 = 0;
                    color48_extra16 = Vector2Int.zero;
                    direction = Vector2Int.zero;
                    break;
            }
        }
    }
}