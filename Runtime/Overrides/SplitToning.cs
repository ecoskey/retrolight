using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Overrides {
    [Serializable, VolumeComponentMenu("PostProcessing/Split Toning"), SupportedOnRenderPipeline(typeof(RetrolightAsset))]
    public class SplitToning : VolumeComponent {
        [Header("SplitToning")] 
        public ColorParameter shadows = new ColorParameter(Color.white, false, false, true);
        public ColorParameter highlights = new ColorParameter(Color.white, false, false, true);
        public ClampedFloatParameter balance = new ClampedFloatParameter(0, -100, 100);
        //todo: is balance clamped 0..1?
    }
}