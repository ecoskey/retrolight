using System;
using Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Util {
    public static class SnappingUtil {
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
            if (!camera.orthographic || camera.GetRetrolightCameraData().PreviousRotation != tf.rotation)
                return new SnappingContext(tf, tf.position, Vector2.zero);
            
            float viewportHeight = 2f * camera.orthographicSize;
            float scale = viewportParams.Resolution.y / viewportHeight;
            Vector3 unSnappedPos = tf.position;

            Vector3 pixelPos = scale * camera.worldToCameraMatrix.MultiplyVector(unSnappedPos);
            Vector3 newPixelPos = new Vector3(
                Mathf.Round(pixelPos.x), 
                Mathf.Round(pixelPos.y), 
                pixelPos.z
            );
            Vector3 newPos = camera.cameraToWorldMatrix.MultiplyVector(newPixelPos / scale);
            tf.position = newPos;

            Vector2 viewportShift = pixelPos - newPixelPos;
            viewportShift.Scale(new Vector2(viewportParams.Resolution.z, viewportParams.Resolution.w));
            
            return new SnappingContext(tf, unSnappedPos, viewportShift);
        }
    }
}
