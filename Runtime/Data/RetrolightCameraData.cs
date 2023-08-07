using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data { 
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class RetrolightCameraData : MonoBehaviour {
        [SerializeField] private bool renderLighting = true;
        [SerializeField] private bool renderShadows = true;
        [SerializeField] private bool usePostFX = true;

        public bool RenderLighting => renderLighting;
        public bool RenderShadows => renderShadows;
        public bool UsePostFX => usePostFX;

        public float3 PreviousPosition { get; private set; }
        public quaternion PreviousRotation { get; private set; }

        private void Awake() {
            var tf = transform;
            PreviousPosition = tf.position;
            PreviousRotation = tf.rotation;
        }

        private void OnPostRender() {
            var tf = transform;
            PreviousPosition = tf.position;
            PreviousRotation = tf.rotation;
        }
    }
}