using System;
using Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Util {
    public static class SnappingUtility {
        public readonly struct SnappingContext : IDisposable {
            private readonly Transform transform;
            private readonly Vector3 unSnappedPos;
            public readonly Vector2 ViewportShift;

            public SnappingContext(Transform transform, Vector3 unSnappedPos, Vector2 viewportShift) {
                this.transform = transform;
                this.unSnappedPos = unSnappedPos;
                ViewportShift = viewportShift;
            }

            public void Dispose() => transform.position = unSnappedPos;
        }

        public static SnappingContext Snap(Camera camera, Transform transform, ViewportParams viewportParams) {
            float viewportHeight = 2f * camera.orthographicSize;
            float scale = viewportParams.Resolution.y / viewportHeight; //todo: integrate pixelScale
            Vector3 unSnappedPos = transform.position;
            
            Vector3 pixelPos = scale * camera.worldToCameraMatrix.MultiplyVector(unSnappedPos);
            Vector3 newPixelPos = new Vector3(
                Mathf.Round(pixelPos.x), 
                Mathf.Round(pixelPos.y), 
                pixelPos.z
            );
            Vector3 newPos = camera.cameraToWorldMatrix.MultiplyVector(newPixelPos / scale);
            transform.position = newPos;

            Vector2 viewportShift = pixelPos - newPixelPos;
            viewportShift.Scale(new Vector2(viewportParams.Resolution.z, viewportParams.Resolution.w));
            
            return new SnappingContext(transform, unSnappedPos, viewportShift);
        }
    }
}
