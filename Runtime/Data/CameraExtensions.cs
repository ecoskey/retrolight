using UnityEngine;

namespace Data {
    public static class CameraExtensions {
        public static RetrolightCameraData GetRetrolightCameraData(this Camera camera) {
            var dataExists = camera.TryGetComponent<RetrolightCameraData>(out var data);
            if (dataExists) return data;
            return camera.gameObject.AddComponent<RetrolightCameraData>();
        }
    }
}