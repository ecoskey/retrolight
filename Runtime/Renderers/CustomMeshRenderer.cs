using Retrolight.Util;
using UnityEditor;
using UnityEngine;

namespace Retrolight.Renderers {
    [RequireComponent(typeof(MeshFilter))]
    public abstract class CustomMeshRenderer : CustomRenderer {
        [SerializeField] protected Material[] materials = new Material[1];

        protected MeshFilter meshFilter;


        private void Awake() => RefreshMesh();

        public void RefreshMesh() {
            meshFilter = GetComponent<MeshFilter>();
        }

        protected RenderParams GetRenderParams(int i, Camera cam = null) {
            var rParams = GetRenderParams(materials[i]);
            rParams.camera = cam;
            return rParams;
        }
        
        protected RenderParams GetRenderParams(int i, Matrix4x4 localToWorld, Camera cam = null) {
            var rParams = GetRenderParams(i, cam);
            rParams.worldBounds = localToWorld.TransformBounds(meshFilter.sharedMesh.bounds);
            return rParams;
        }
    }
}