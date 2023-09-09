using System;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Retrolight.Util {
    public static class MathUtils {
        /*public static int NextMultipleOf(int n, int factor) {
            if (n <= 0) return 1;
            if (n % factor == 0) return n;
            return n + (factor - (n % factor));
        }*/

        //returns minimum x such that x * factor >= n
        /*public static int NextMultipleOf(int n, int factor) {
            if (n <= 0) return 1;
            if (n % factor == 0) return n / factor;
            return n / factor + 1;
        }*/

        public static int NextMultipleOf(int n, int factor) =>
            Math.Max(1, (n + factor - 1) / factor);
        
        public static int2 AsVector(this Vector2Int v) => int2(v.x, v.y);
        public static int3 AsVector(this Vector3Int v) => int3(v.x, v.y, v.z);
        
        public static float4 AsVector(this Color c) => float4(c.r, c.g, c.b, c.a);

        public static float4x4 AsMatrix(this Matrix4x4 m) => float4x4(
            m.m00, m.m01, m.m02, m.m03,
            m.m10, m.m11, m.m12, m.m13,
            m.m20, m.m21, m.m22, m.m23,
            m.m30, m.m31, m.m32, m.m33
        );
        
        public static float3x3 Truncate(this float4x4 m) => float3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static double3x3 Truncate(this double4x4 m) => double3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static int3x3 Truncate(this int4x4 m) => int3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
        public static uint3x3 Truncate(this uint4x4 m) => uint3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);

        public static Bounds TransformBounds(this Matrix4x4 m, Bounds bounds) {
            var min = bounds.min;
            var max = bounds.max;

            Vector3[] corners = new Vector3[8] {
                min,
                new(min.x, min.y, max.z),
                new(min.x, max.y, min.z),
                new(min.x, max.y, max.z),
                new(max.x, min.y, min.z),
                new(max.x, min.y, max.z),
                new(max.x, max.y, min.z),
                max,
            };

            min = new Vector3(INFINITY, INFINITY, INFINITY);
            max = new Vector3(-INFINITY, -INFINITY, -INFINITY);

            for (int i = 0; i < 8; i++) {
                var newCorner = m.MultiplyPoint(corners[i]);
                min = Vector3.Min(min, newCorner);
                max = Vector3.Max(max, newCorner);
            }

            bounds.SetMinMax(min, max);
            return bounds;
        }
    }
}