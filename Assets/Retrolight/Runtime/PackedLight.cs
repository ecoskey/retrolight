using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using LightType = UnityEngine.LightType;

namespace Retrolight.Runtime {
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedLight {
        private Vector3 position;
        private uint type16_range16;
        //todo: hopefully hlsl structs are packed in a way to make this work
        private uint color32, color16_extra16;
        private uint direction0, direction1;

        public const int Stride = 
            sizeof(float) * 3 + 
            sizeof(uint) + 
            sizeof(int) * 2 + 
            sizeof(int) * 2;
        
        private enum PackedLightType {
            Directional = 0,
            Point = 1,
            Spot = 2,
            Line = 3
        }

        public PackedLight(VisibleLight light) {
            switch (light.lightType) {
                case LightType.Directional:
                    position = Vector3.zero;
                    type16_range16 = (uint) PackedLightType.Directional | (uint) Mathf.FloatToHalf(light.range) << 16;
                    color32 = Mathf.FloatToHalf(light.finalColor.r) | (uint) Mathf.FloatToHalf(light.finalColor.g) << 16;
                    color16_extra16 = Mathf.FloatToHalf(light.finalColor.b);
                    //todo: get direction vector of light, rather than finalcolor for direction
                    direction0 = Mathf.FloatToHalf(light.finalColor.r) | (uint) Mathf.FloatToHalf(light.finalColor.g) << 16;
                    direction1 = Mathf.FloatToHalf(light.finalColor.b);
                    break;
                default: 
                    position = Vector3.zero;
                    type16_range16 = color32 = color16_extra16 = direction0 = direction1 = 0;
                    break;
            }
        }
    }
}