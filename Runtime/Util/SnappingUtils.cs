using System;
using Retrolight.Data;
using UnityEngine;

namespace Retrolight.Util {
    public static class SnappingUtils {
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

        public static SnappingContext SnapCamera(Camera camera, ViewportParams viewportParams) {
            var tf = camera.transform;
            Vector3 unSnappedPos = tf.position;
            if (!camera.orthographic || camera.GetRetrolightCameraData().PreviousRotation != tf.rotation)
                return new SnappingContext(tf, unSnappedPos, Vector2.zero);
            
            float viewportHeight = 2f * camera.orthographicSize;
            float scale = viewportParams.Resolution.y / viewportHeight;
            
            Vector3 pixelPos = scale * camera.worldToCameraMatrix.MultiplyVector(unSnappedPos);
            Vector3 newPixelPos = new Vector3(
                Mathf.Round(pixelPos.x), 
                Mathf.Round(pixelPos.y), 
                pixelPos.z
            );
            
            tf.position = camera.cameraToWorldMatrix.MultiplyVector(newPixelPos / scale);
            
            Vector2 viewportShift = new Vector2(
                (pixelPos.x - newPixelPos.x) * viewportParams.Resolution.z,
                (pixelPos.y - newPixelPos.y) * viewportParams.Resolution.w
            );

            return new SnappingContext(tf, unSnappedPos, viewportShift);
        }
    }
}
