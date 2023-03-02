using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        private Vector3 position;

        private uint type2_flags6_shadowIndex8_range16;

        private uint color32, color16_cosAngle16;
        private uint direction32, direction16_shadowStrength16;

        public const int Stride =
            sizeof(float) * 3 +
            sizeof(uint) +
            sizeof(int) * 2 +
            sizeof(int) * 2;

        private enum PackedLightType : byte {
            Directional = 0,
            Point = 1,
            Spot = 2,
            Line = 3
        }

        [Flags]
        private enum PackedLightFlags : byte { //yes, it's 6 bits long (oddball)
            None = 0,
            Shadowed = 0b000001
        }

        public PackedLight(VisibleLight light, byte shadowIndex) {
            color32 = PackFloats(light.finalColor.r, light.finalColor.g);
            Vector3 dir = -light.localToWorldMatrix.GetColumn(2);
            switch (light.lightType) {
                case LightType.Directional:
                    position = Vector3.zero;
                    type2_flags6_shadowIndex8_range16 = PackTypeField(
                        PackedLightType.Directional, GetLightFlags(light), 
                        shadowIndex, light.range
                    );
                    direction32 = PackFloats(dir.x, dir.y);
                    direction16_shadowStrength16 = PackFloats(dir.z, light.light.shadowStrength);
                    color16_cosAngle16 = Mathf.FloatToHalf(light.finalColor.b);
                    break;
                case LightType.Point: 
                    position = light.localToWorldMatrix.GetPosition();
                    type2_flags6_shadowIndex8_range16 = PackTypeField(
                        PackedLightType.Point, GetLightFlags(light), 
                        shadowIndex, light.range
                    );
                    direction32 = 0;
                    direction16_shadowStrength16 = PackFloats(0, light.light.shadowStrength);
                    color16_cosAngle16 = Mathf.FloatToHalf(light.finalColor.b);
                    break;
                case LightType.Spot:
                    position = light.localToWorldMatrix.GetPosition();
                    type2_flags6_shadowIndex8_range16 = PackTypeField(
                        PackedLightType.Spot, GetLightFlags(light), 
                        shadowIndex, light.range
                    );
                    direction32 = PackFloats(dir.x, dir.y);
                    direction16_shadowStrength16 = PackFloats(dir.z, light.light.shadowStrength);
                    color16_cosAngle16 = 
                        PackFloats(light.finalColor.b, Mathf.Cos(Mathf.Deg2Rad * light.spotAngle * 0.5f));
                    break;
                default:
                    position = Vector3.zero;
                    type2_flags6_shadowIndex8_range16 = color32 = color16_cosAngle16 = 
                        direction32 = direction16_shadowStrength16 = 0;
                    break;
            }
        }

        private static PackedLightFlags GetLightFlags(VisibleLight light) {
            PackedLightFlags flags = PackedLightFlags.None;
            if (light.light.shadows != LightShadows.None && light.light.shadowStrength > 0)
                flags |= PackedLightFlags.Shadowed;
            return flags;
        }

        private static uint PackFloats(float n1, float n2) {
            return Mathf.FloatToHalf(n1) | (uint) Mathf.FloatToHalf(n2) << 16;
        }

        private static uint PackTypeField(
            PackedLightType type, PackedLightFlags flags,
            byte shadowIndex, float range
        ) {
            return (uint) (
                (byte) type | ((byte) flags & 0x3F) << 2 | 
                shadowIndex << 8 | Mathf.FloatToHalf(range) << 16
            );
        }
    }
}