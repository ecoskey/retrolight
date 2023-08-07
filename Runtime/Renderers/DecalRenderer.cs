using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renderers {
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class DecalRenderer : MonoBehaviour {
        private Mesh decalMesh;
        private Material decalMaterial;
        
        private void Awake() {
            decalMesh = CoreUtils.CreateCubeMesh(float3(0), float3(1));
            decalMaterial = CoreUtils.CreateEngineMaterial("Retrolight/Decal");
            RefreshProperties();
        }

        //private void OnValidate() => RefreshProperties();

        private void OnDestroy() {
            CoreUtils.Destroy(decalMesh);
            CoreUtils.Destroy(decalMaterial);
        }

        private void RefreshProperties() {
            GetComponent<MeshFilter>().mesh = decalMesh;
            //GetComponent<MeshRenderer>().sharedMaterial = decalMaterial;
        }
    }
}