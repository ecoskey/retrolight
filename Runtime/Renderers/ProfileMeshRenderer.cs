using System;
using UnityEngine;

namespace Renderers {
    [RequireComponent(typeof(MeshFilter))]
    public class ProfileMeshRenderer : MonoBehaviour {
        [SerializeField] private Material[] materials;

        private MeshFilter meshFilter;

        [Range(0, 1)] private float skewStrength;

        private void Awake() {
            meshFilter = GetComponent<MeshFilter>();
        }


        private void Update() {
            //Graphics.DrawMesh(meshFilter.mesh, );
        }

        private RenderParams GetRenderParams(int i) => new RenderParams(materials[i]) {
            
        };

        // ReSharper disable once ParameterHidesMember
        private Matrix4x4 GetSkewedModelMatrix(Matrix4x4 m, Camera camera) {
            //world to camera or camera to world? I think the second one
            Vector3 cameraForward = -camera.cameraToWorldMatrix.GetColumn(2); // negate? wait is this fine actually?
            Vector3 skewAxis = new Vector3(cameraForward.x, -skewStrength * cameraForward.y, cameraForward.z).normalized;

            // this isn't right, wah wah
            
            m.m00 = skewAxis.x;
            m.m10 = skewAxis.y;

            return m;
        }
    }
}