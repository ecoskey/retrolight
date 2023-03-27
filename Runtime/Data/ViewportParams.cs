using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data {
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewportParams {
        private const float tileSize = 8f;
        
        public readonly Vector4 Resolution; // .xy is resolution, .zw is reciprocal resolution
        public readonly Vector2Int PixelCount;
        public readonly Vector2Int TileCount;
        public readonly Vector2 ViewportScale;

        public ViewportParams(RTHandleProperties rtHandleProperties) {
            PixelCount = rtHandleProperties.currentViewportSize;
            Resolution = new Vector4(PixelCount.x, PixelCount.y, 1f / PixelCount.x, 1f / PixelCount.y);
            TileCount = new Vector2Int(
                Mathf.CeilToInt(PixelCount.x / tileSize), 
                Mathf.CeilToInt(PixelCount.y / tileSize)
            );
            var scale = rtHandleProperties.rtHandleScale;
            ViewportScale = new Vector2(scale.x, scale.y);
        }
    }
}