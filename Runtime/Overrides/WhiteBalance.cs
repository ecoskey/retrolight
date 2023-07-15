using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Overrides {
    [Serializable, VolumeComponentMenuForRenderPipeline("PostProcessing/White Balance", typeof(Retrolight))]
    public class WhiteBalance : VolumeComponent {
        [Header("White Balance")] 
        public ClampedFloatParameter temperature = new ClampedFloatParameter(0, -100, 100);
        public ClampedFloatParameter tint = new ClampedFloatParameter(0, -100, 100);
    }
}