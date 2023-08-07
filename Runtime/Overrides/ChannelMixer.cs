using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Rendering;

namespace Overrides {
    [Serializable, VolumeComponentMenu("PostProcessing/Channel Mixer"), SupportedOnRenderPipeline(typeof(RetrolightAsset))]
    public class ChannelMixer : VolumeComponent {
        [Header("Channel Mixer")]
        public Vector3Parameter red = new Vector3Parameter(Vector3.right);
        public Vector3Parameter green = new Vector3Parameter(Vector3.up);
        public Vector3Parameter blue = new Vector3Parameter(Vector3.forward);

        public float3x3 MixerMatrix => float3x3(red.value, green.value, blue.value);
    }
}