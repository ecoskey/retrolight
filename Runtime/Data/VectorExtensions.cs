using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Data {
    public static class VectorExtensions {
        public static int2 AsVector(this Vector2Int v) => int2(v.x, v.y);
        public static int3 AsVector(this Vector3Int v) => int3(v.x, v.y, v.z);
        public static float4 AsVector(this Color c) => float4(c.r, c.g, c.b, c.a);

        public static float4x4 AsMatrix(this Matrix4x4 m) => float4x4(
            m.m00, m.m01, m.m02, m.m03,
            m.m10, m.m11, m.m12, m.m13,
            m.m20, m.m21, m.m22, m.m23,
            m.m30, m.m31, m.m32, m.m33
        );
        
        public static float3x3 As3DMatrix(this float4x4 m) => float3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static double3x3 As3DMatrix(this double4x4 m) => double3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static int3x3 As3DMatrix(this int4x4 m) => int3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static uint3x3 As3DMatrix(this uint4x4 m) => uint3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
    }
}
