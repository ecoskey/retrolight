using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        private Vector3 position;
        
        private byte type2_flags6;
        private byte shadowIndex;
        private ushort range;
        
        public byte ShadowIndex { set => shadowIndex = value; }

        private ushort colorR, colorG, colorB;
        private ushort cosAngle;

        private ushort dirX, dirY, dirZ;
        private ushort shadowStrength;

        public const int Stride =
            sizeof(float) * 3
            + sizeof(byte)
            + sizeof(byte)
            + sizeof(ushort)
            + sizeof(ushort) * 3
            + sizeof(ushort)
            + sizeof(ushort) * 3
            + sizeof(ushort);

        private enum PackedLightType : byte {
            Directional = 0,
            Point = 1,
            Spot = 2,
        }

        [Flags]
        private enum PackedLightFlags : byte {
            None = 0,
            Shadowed = 0b000001
        }

        public PackedLight(VisibleLight light, byte shadowIndex) {
            position = light.localToWorldMatrix.GetPosition();
            
            type2_flags6 = (byte) ((byte) GetLightType(light) | (byte) GetLightFlags(light) << 2);
            range = Mathf.FloatToHalf(light.range);
            this.shadowIndex = shadowIndex;
            
            colorR = Mathf.FloatToHalf(light.finalColor.r);
            colorG = Mathf.FloatToHalf(light.finalColor.g);
            colorB = Mathf.FloatToHalf(light.finalColor.b);
            
            cosAngle = Mathf.FloatToHalf(Mathf.Cos(Mathf.Deg2Rad * light.spotAngle * 0.5f));
            
            Vector3 dir = -light.localToWorldMatrix.GetColumn(2);
            dirX = Mathf.FloatToHalf(dir.x);
            dirY = Mathf.FloatToHalf(dir.y);
            dirZ = Mathf.FloatToHalf(dir.z);
            shadowStrength = Mathf.FloatToHalf(light.light.shadowStrength);
        }

        private static PackedLightFlags GetLightFlags(VisibleLight light) {
            PackedLightFlags flags = PackedLightFlags.None;
            if (light.light.shadows != LightShadows.None && light.light.shadowStrength > 0)
                flags |= PackedLightFlags.Shadowed;
            return flags;
        }

        private static PackedLightType GetLightType(VisibleLight light) => light.lightType switch {
            LightType.Directional => PackedLightType.Directional,
            LightType.Point => PackedLightType.Point,
            LightType.Spot => PackedLightType.Spot,
            _ => PackedLightType.Spot //todo: support area lights in future
        };
    }
}