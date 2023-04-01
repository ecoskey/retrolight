using System;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "Retrolight PostFX Settings", menuName = "Retrolight/PostFX Settings", order = 1)]
    public class PostFxSettings : ScriptableObject {
        [Serializable]
        public struct BloomSettings {
            [Range(0f, 16f)] private int maxIterations;
            public int MaxIterations => maxIterations;

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
        
        [SerializeField] private BloomSettings bloom = default;
        public BloomSettings Bloom => bloom;
    }
}