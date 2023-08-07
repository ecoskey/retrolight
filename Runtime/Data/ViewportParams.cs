using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Rendering;
using Util;
using int2 = Unity.Mathematics.int2;

namespace Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewportParams {
        public readonly float4 Resolution; // .xy is resolution, .zw is reciprocal resolution
        public readonly int2 PixelCount;
        public readonly int2 TileCount;
        public readonly float2 ViewportScale;

        public ViewportParams(RTHandleProperties rtHandleProperties) {
            var rawPixels = rtHandleProperties.currentViewportSize;
            PixelCount = int2(rawPixels.x, rawPixels.y);
            Resolution = float4(PixelCount.x, PixelCount.y, 1f / PixelCount.x, 1f / PixelCount.y);
            TileCount = int2(
                MathUtils.NextMultipleOf(PixelCount.x, Constants.MediumTile), 
                MathUtils.NextMultipleOf(PixelCount.y, Constants.MediumTile)
            );
            var scale = rtHandleProperties.rtHandleScale;
            ViewportScale = float2(scale.x, scale.y);
        }
    }
}