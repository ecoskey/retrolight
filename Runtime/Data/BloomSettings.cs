using System;
using UnityEngine;


namespace Data {
    [Serializable]
    public struct BloomSettings {
        [SerializeField] private bool enableBloom;
        [SerializeField, Range(0, 16)] private int maxIterations;
        [SerializeField, Min(1)] private int downscaleLimit;
        //[SerializeField] private bool highQuality;
        [SerializeField, Min(0)] private float threshold;
        [SerializeField, Range(0, 1)] private float knee;
        [SerializeField, Min(0)] private float intensity;

        public bool EnableBloom => enableBloom;
        public int MaxIterations => maxIterations;
        public int DownscaleLimit => downscaleLimit;
        //public bool HighQuality => highQuality;
        public float Threshold => threshold;
        public float Knee => knee;
        public float Intensity => intensity;
        
        public Vector4 ThresholdParams {
            get {
                var thresholdKnee = threshold * knee;
                return new Vector4(
                    threshold, -threshold + thresholdKnee,
                    2 * thresholdKnee, 1f / (4 * thresholdKnee + 1e-5f)
                );
            }
        }

        public BloomSettings(
            bool enableBloom, int maxIterations, int downscaleLimit, //bool highQuality, 
            float threshold, float knee, float intensity
        ) {
            // ReSharper disable InconsistentNaming
            var _maxIterations = Math.Clamp(maxIterations, 0, 16);
            var _downScaleLimit = Math.Max(downscaleLimit, 1);
            var _threshold = Math.Max(threshold, 1);
            var _knee = Math.Clamp(knee, 0, 1);
            var _intensity = Math.Max(intensity, 0);

            this.enableBloom = enableBloom;
            this.maxIterations = _maxIterations;
            this.downscaleLimit = _downScaleLimit;
            //this.highQuality = highQuality;
            this.threshold = _threshold;
            this.knee = _knee;
            this.intensity = _intensity;
        }
    }
}