using System.Runtime.InteropServices;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewportParams {
        public readonly Vector4 Resolution; // .xy is resolution, .zw is reciprocal resolution
        public readonly Vector2Int PixelCount;
        public readonly Vector2Int TileCount;
        public readonly Vector2 ViewportScale;

        public ViewportParams(RTHandleProperties rtHandleProperties) {
            var rawPixels = rtHandleProperties.currentViewportSize;
            PixelCount = new Vector2Int(rawPixels.x, rawPixels.y);
            Resolution = new Vector4(PixelCount.x, PixelCount.y, 1f / PixelCount.x, 1f / PixelCount.y);
            TileCount = new Vector2Int(
                MathUtils.NextMultipleOf(PixelCount.x, Constants.MediumTile), 
                MathUtils.NextMultipleOf(PixelCount.y, Constants.MediumTile)
            );
            var scale = rtHandleProperties.rtHandleScale;
            ViewportScale = new Vector2(scale.x, scale.y);
        }
    }
}