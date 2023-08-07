using System;
using Data;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Rendering;
using float2 = Unity.Mathematics.float2;

namespace Util {
    public static class SnappingUtils {
        public readonly struct SnappingContext : IDisposable {
            private readonly Transform transform;
            private readonly float3 unSnappedPos;
            public readonly float2 ViewportShift;

            public SnappingContext(Transform transform, float3 unSnappedPos, float2 viewportShift) {
                this.transform = transform;
                this.unSnappedPos = unSnappedPos;
                ViewportShift = viewportShift;
            }

            public void Dispose() => transform.position = unSnappedPos;
        }

        public static SnappingContext SnapCamera(Camera camera, ViewportParams viewportParams) {
            var tf = camera.transform;
            float3 unSnappedPos = tf.position;
            if (!camera.orthographic || camera.GetRetrolightCameraData().PreviousRotation != tf.rotation)
                return new SnappingContext(tf, unSnappedPos, float2(0));
            
            float viewportHeight = 2f * camera.orthographicSize;
            float scale = viewportParams.Resolution.y / viewportHeight;

            float3x3 worldToCamera = camera.worldToCameraMatrix.AsMatrix().As3DMatrix();
            float3x3 cameraToWorld = camera.cameraToWorldMatrix.AsMatrix().As3DMatrix();
            
            float3 pixelPos = scale * mul(worldToCamera, unSnappedPos);
            float3 newPixelPos = float3(
                Mathf.Round(pixelPos.x), 
                Mathf.Round(pixelPos.y), 
                pixelPos.z
            );
            
            tf.position = mul(cameraToWorld, newPixelPos / scale);
            float2 viewportShift = (pixelPos - newPixelPos).xy * viewportParams.Resolution.zw;
            return new SnappingContext(tf, unSnappedPos, viewportShift);
        }
    }
}
