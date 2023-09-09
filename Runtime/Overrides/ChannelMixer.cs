using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Overrides {
    [Serializable, VolumeComponentMenu("PostProcessing/Channel Mixer"), SupportedOnRenderPipeline(typeof(RetrolightAsset))]
    public class ChannelMixer : VolumeComponent {
        [Header("Channel Mixer")]
        public Vector3Parameter red = new Vector3Parameter(Vector3.right);
        public Vector3Parameter green = new Vector3Parameter(Vector3.up);
        public Vector3Parameter blue = new Vector3Parameter(Vector3.forward);

        //public Matrix4x4 MixerMatrix => new Matrix4x4(red.value, green.value, blue.value);
    }
}