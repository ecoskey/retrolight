using System;
using UnityEngine;

namespace Data { 
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class RetrolightCameraData : MonoBehaviour {
        [SerializeField] private bool renderLighting;
        [SerializeField] private bool renderShadows;
        [SerializeField] private bool enablePostFX;
        [SerializeField] private PostFxSettings postFxSettings;

        public bool RenderLighting => renderLighting;
        public bool RenderShadows => renderShadows;
        public bool EnablePostFx => enablePostFX;
        public PostFxSettings PostFxSettings => postFxSettings;

        public Vector3 PreviousPosition { get; private set; }
        public Quaternion PreviousRotation { get; private set; }

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