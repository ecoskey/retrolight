using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Data {
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct GTAOSettings {
        [SerializeField, Range(0.1f, 1f)] public float Radius;
        //[Range(0.05f, 0.5f)] public float FalloffRange;
    }
}