using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Overrides {
    [Serializable, VolumeComponentMenu("PostProcessing/Color Adjustments"), SupportedOnRenderPipeline(typeof(RetrolightAsset))]
    public class ColorAdjustments : VolumeComponent {
        [Header("Color Adjustments")]
        public FloatParameter postExposure = new FloatParameter(0);
        public ClampedFloatParameter contrast = new ClampedFloatParameter(0, -100, 100);
        public ClampedFloatParameter saturation = new ClampedFloatParameter(0, -100, 100);
        public ColorParameter colorFilter = new ColorParameter(Color.white, false, true, false);
        public ClampedFloatParameter hueShift = new ClampedFloatParameter(0, -180, 180);
    }
}