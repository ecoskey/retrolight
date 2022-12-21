using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Util {
    /*public static class CameraExtensions {
        public static PixelPerfectCamera GetPixelPerfectCamera(this Camera camera) {
            GameObject gameObject = camera.gameObject;
            var hasComponent = gameObject.TryGetComponent(out PixelPerfectCamera pixelPerfectCamera);
            if (!hasComponent) pixelPerfectCamera = gameObject.AddComponent<PixelPerfectCamera>();
            return pixelPerfectCamera;
        }
    }*/
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class PixelPerfectCamera : MonoBehaviour {
        private Vector3 unSnappedPos;
        private Vector2 snapDist; // in fractions of render texture width/height

        private new Camera camera;

        private void Awake() {
            camera = GetComponent<Camera>();
        }

        private void Snap() {
            Transform tf = transform;
            unSnappedPos = tf.position;
            Vector3 eulerAngles = tf.rotation.eulerAngles;

            float
                sinX = Mathf.Sin(eulerAngles.x), // x is "vertical" rotation
                cosX = Mathf.Sin(eulerAngles.x),
                sinY = Mathf.Sin(eulerAngles.y), // y is "horizontal" rotation
                cosY = Mathf.Sin(eulerAngles.y);
            
            
        }

        private void Unsnap() {
            transform.position = unSnappedPos;
        }
    }
}
