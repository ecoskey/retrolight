using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Overrides {
    [Serializable, VolumeComponentMenuForRenderPipeline("PostProcessing/Channel Mixer", typeof(Retrolight))]
    public class ChannelMixer : VolumeComponent {
        [Header("Channel Mixer")]
        public Vector3Parameter red = new Vector3Parameter(Vector3.right);
        public Vector3Parameter green = new Vector3Parameter(Vector3.up);
        public Vector3Parameter blue = new Vector3Parameter(Vector3.forward);
    }
}