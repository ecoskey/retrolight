using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Rendering;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Data {
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PackedLight {
        private readonly float3 position;
        
        private readonly byte type2_flags6;
        private readonly byte shadowIndex;
        private readonly half range;

        private readonly half3 color;
        private readonly half cosAngle;

        private readonly half3 dir;
        private readonly half shadowStrength;

        private enum PackedLightType : byte {
            Directional = 0,
            Point = 1,
            Spot = 2,
        }

        [Flags]
        private enum PackedLightFlags : byte {
            None = 0,
            Shadowed = 1
        }

        public PackedLight(VisibleLight light, byte shadowIndex) {
            position = light.localToWorldMatrix.GetPosition();
            
            type2_flags6 = (byte) ((byte) GetLightType(light) | (byte) GetLightFlags(light) << 2);
            range = half(light.range);
            this.shadowIndex = shadowIndex;

            var rawColor = light.finalColor;
            color = half3(float3(rawColor.r, rawColor.g, rawColor.b));

            cosAngle = half(Mathf.Cos(Mathf.Deg2Rad * light.spotAngle * 0.5f));
            
            dir = half3((Vector3) (-light.localToWorldMatrix.GetColumn(2)));
            shadowStrength = half(light.light.shadowStrength);
        }

        private static PackedLightFlags GetLightFlags(VisibleLight light) {
            var flags = PackedLightFlags.None;
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