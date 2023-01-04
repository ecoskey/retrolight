using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        private Vector3 position;

        private uint type16_range16;

        //todo: hopefully hlsl structs are packed in a way to make this work
        private uint color32, color16_cosAngle16;
        private uint direction0, direction1;

        public const int Stride =
            sizeof(float) * 3 +
            sizeof(uint) +
            sizeof(int) * 2 +
            sizeof(int) * 2;

        private enum PackedLightType {
            Directional = 0,
            Point = 1,
            Spot = 2,
            Line = 3
        }

        public PackedLight(VisibleLight light) {
            color32 = Mathf.FloatToHalf(light.finalColor.r) | (uint) Mathf.FloatToHalf(light.finalColor.g) << 16;
            color16_cosAngle16 = Mathf.FloatToHalf(light.finalColor.b);
            Vector3 dir = -light.localToWorldMatrix.GetColumn(2);
            switch (light.lightType) {
                case LightType.Directional:
                    position = Vector3.zero;
                    type16_range16 = (uint) PackedLightType.Directional;
                    direction0 = Mathf.FloatToHalf(dir.x) | (uint) Mathf.FloatToHalf(dir.y) << 16;
                    direction1 = Mathf.FloatToHalf(dir.z);
                    break;
                case LightType.Point: 
                    position = light.localToWorldMatrix.GetPosition();
                    type16_range16 = (uint) PackedLightType.Point | (uint) Mathf.FloatToHalf(light.range) << 16;
                    direction0 = direction1 = 0;
                    break;
                case LightType.Spot:
                    position = light.localToWorldMatrix.GetPosition();
                    type16_range16 = (uint) PackedLightType.Spot | (uint) Mathf.FloatToHalf(light.range) << 16;
                    direction0 = Mathf.FloatToHalf(dir.x) | (uint) Mathf.FloatToHalf(dir.y) << 16;
                    direction1 = Mathf.FloatToHalf(dir.z);
                    color16_cosAngle16 |= (uint) 
                        Mathf.FloatToHalf(Mathf.Cos(Mathf.Deg2Rad * light.spotAngle * 0.5f)) << 16;
                    break;
                default:
                    position = Vector3.zero;
                    type16_range16 = color32 = color16_cosAngle16 = direction0 = direction1 = 0;
                    break;
            }
        }
    }
}