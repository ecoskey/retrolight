using System;
using Retrolight.Data;
using UnityEngine;

namespace Retrolight.Util {
    public static class SnappingUtility {
        public readonly struct SnappingContext : IDisposable {
            private readonly Transform tf;
            private readonly Vector3 unSnappedPos;

            public SnappingContext(Transform tf, Vector3 unSnappedPos) {
                this.tf = tf;
                this.unSnappedPos = unSnappedPos;
            }

            public void Dispose() => tf.position = unSnappedPos;
        }

        private static SnappingContext Snap(Transform tf, Vector2 pixelScale) {
            Vector3 unSnappedPos = tf.position;
            Vector3 eulerAngles = tf.rotation.eulerAngles;

            float
                sinX = Mathf.Sin(eulerAngles.x), // x is "vertical" rotation
                cosX = Mathf.Sin(eulerAngles.x),
                sinY = Mathf.Sin(eulerAngles.y), // y is "horizontal" rotation
                cosY = Mathf.Sin(eulerAngles.y);

            Matrix2x3 worldToPixel = new Matrix2x3(
                cosY,        0,    -sinY,
                sinX * sinY, cosX, cosY * cosY
            );

            Matrix2x2 pixelToWorld = new Matrix2x2( //in the context of the derivative of position with respect to time
                cosY * cosY, sinY,
                -sinX * sinY, cosY
            );
            
            float pixelToWorldDet = 1f / (Mathf.Pow(cosY, 3) + sinX * sinY * sinY);
            
            Vector2 pixelPos = pixelScale.x * (worldToPixel * unSnappedPos);
            Vector2 dUdV = new Vector2(pixelPos.x % 1f, pixelPos.y % 1f);
            Vector2 dXdZ = pixelScale.y * pixelToWorldDet * (pixelToWorld * -dUdV);
            tf.Translate(dXdZ.x, 0, dXdZ.y);
            return new SnappingContext(tf, unSnappedPos);
        }
    }
}
