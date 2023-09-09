using System;
using System.Collections.Generic;
using System.Linq;
using Retrolight.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Renderers {
    [ExecuteAlways]
    public class SkewedMeshRenderer : CustomMeshRenderer {
        //private Dictionary<Camera, Matrix4x4> cachedSkewMatrices;
        [Tooltip("The strength of the skew effect applied")]
        [SerializeField, Range(0, 1)] private float skewStrength;
        [Tooltip("The center of the skew effect. Set this if the visual center of your mesh doesn't match transform.position")]
        [SerializeField] private Option<Vector3> skewOrigin;

        public void OnEnable() { RenderPipelineManager.beginContextRendering += RenderSkewedMeshes; }
        private void OnDisable() { RenderPipelineManager.beginContextRendering -= RenderSkewedMeshes; }
        


        private Matrix4x4 GetSkewMatrix(Camera cam) {
            Vector3 viewDir = cam.orthographic ? 
                -cam.cameraToWorldMatrix.GetColumn(2) : 
                (transform.position + skewOrigin.OrElse(Vector3.zero) - cam.transform.position).normalized;
            viewDir.y *= skewStrength;

            var xyProj = new Vector3(1, viewDir.y * viewDir.x, 0);
            var zyProj = new Vector3(0, viewDir.y * viewDir.z, 1);
            
            Matrix4x4 skewMatrix = Matrix4x4.identity;

            skewMatrix.SetColumn(0, xyProj);
            skewMatrix.SetColumn(2, zyProj);
            
            //pre- and post-multiply matrices with TRS matrix?
            //could just premultiply, but could cause issues
            //also for orthographic cameras it should be recalculated unless the camera turns
            return skewMatrix;
        }

        private void RenderSkewedMeshes(ScriptableRenderContext ctx, List<Camera> cams) {

            var mesh = meshFilter.sharedMesh;
            int renderCount = Math.Min(mesh.subMeshCount, materials.Length);
            
            if (skewStrength == 0) {
                for (int i = 0; i < renderCount; i++) {
                    Graphics.RenderMesh(GetRenderParams(materials[i]),
                        mesh, i, transform.localToWorldMatrix
                    );
                }
            }

            foreach (var cam in cams) {
                //use main camera to visualize effect, not scene view one
                #if UNITY_EDITOR
                var actualCam =
                    SceneView.currentDrawingSceneView is not null
                    && SceneView.currentDrawingSceneView.camera is not null
                    && cam == SceneView.currentDrawingSceneView.camera
                        ? Camera.main
                        : cam;
                #else
                var actualCam = cam;
                #endif
                    
                var skewMatrix = GetSkewMatrix(actualCam);
                for (int i = 0; i < renderCount; i++) {
                    if (materials[i] is null) continue;
                    var customLocaltoWorld = skewMatrix * transform.localToWorldMatrix;
                    Graphics.RenderMesh(
                        //new RenderParams(materials[i]),
                        GetRenderParams(i, customLocaltoWorld, cam),
                        mesh, i, customLocaltoWorld
                    );
                }
            }
        }
    }
}