using System;
using System.Runtime.InteropServices;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Retrolight.Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        [Flags]
        public enum Flags : byte {
            None = 0,
            Shadowed = 1
        }

        public enum Type : byte {
            Directional = 0,
            Point = 1,
            Spot = 2,
        }

        public float3 position;
        
        public byte type2_flags6;
        public byte shadowIndex;
        public half range;

        public half3 color;
        public half cosAngle;

        public half3 dir;
        public half shadowStrength;
    }
}