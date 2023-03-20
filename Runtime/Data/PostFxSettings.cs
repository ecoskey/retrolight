using System;
using UnityEngine;

namespace Retrolight.Data {
    [CreateAssetMenu(menuName = "Rendering/Custom Post FX Settings")]
    public class PostFxSettings : ScriptableObject {
        [Serializable]
        public struct BloomSettings {
            [Range(0f, 16f)] private int maxIterations;
            public int MaxIterations1 => maxIterations;

            [Min(1f)] private int downscaleLimit;
            public int DownscaleLimit => downscaleLimit;

            private bool bicubicUpsampling;
            public bool BicubicUpsampling => bicubicUpsampling;

            [Min(0f)] private float threshold;
            public float Threshold => threshold;

            [Range(0f, 1f)] private float thresholdKnee;
            public float ThresholdKnee => thresholdKnee;


            [Min(0f)] private float intensity;
            public float Intensity => intensity;
        }
        
        [SerializeField] private BloomSettings bloom;
        public BloomSettings Bloom => bloom;
    }
}