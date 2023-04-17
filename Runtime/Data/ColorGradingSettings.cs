using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Data {
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct ColorGradingSettings {
        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct SplitToningSettings {
            private Vector3 shadows;
            private Vector3 highlights;
            private float balance;
        }

        private struct WhiteBalanceSettings {
            
        }

        private float exposure;
    }
}