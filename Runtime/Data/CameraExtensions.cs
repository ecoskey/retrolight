using UnityEditor;
using UnityEngine;

namespace Retrolight.Data {
    public static class CameraExtensions {
        public static RetrolightCameraData GetRetrolightCameraData(this Camera camera) {
            var dataExists = camera.TryGetComponent<RetrolightCameraData>(out var data);
            if (dataExists) return data;
            return camera.gameObject.AddComponent<RetrolightCameraData>();
        }

        #if UNITY_EDITOR
        public static bool IsSceneView(this Camera camera, out SceneView currentSceneView) {
            if (SceneView.currentDrawingSceneView is not null) {
                currentSceneView = SceneView.currentDrawingSceneView;
                return currentSceneView.camera is not null 
                    && currentSceneView.camera == camera;
            }
            currentSceneView = null;
            return false;
        }
        #endif
        
        public static bool IsSceneView(this Camera camera) {
            #if UNITY_EDITOR
            return SceneView.currentDrawingSceneView is not null
                && SceneView.currentDrawingSceneView.camera is not null
                && SceneView.currentDrawingSceneView.camera == camera;
            #else
            return false;
            #endif
        }
    }
}