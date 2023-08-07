using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace Util {
    public static class RenderUtils {
        //moved from CoreUtils because it still uses legacy commandBuffers
        /*public static void DrawRendererList(RasterCommandBuffer cmd, RendererList rendererList) {
            if (!rendererList.isValid)
                throw new ArgumentException("Invalid renderer list provided to DrawRendererList");

            cmd.DrawRendererList(rendererList);
        }
        
        public static void DrawRendererList(LowLevelCommandBuffer cmd, RendererList rendererList) {
            if (!rendererList.isValid)
                throw new ArgumentException("Invalid renderer list provided to DrawRendererList");

            cmd.DrawRendererList(rendererList);
        }*/

        public static void DispatchCompute(
            ComputeCommandBuffer cmd, ComputeShader computeShader, 
            int kernelIndex, int3 groups
        ) => cmd.DispatchCompute(computeShader, kernelIndex, groups.x, groups.y, groups.z);
        
        public static void DispatchCompute(
            CommandBuffer cmd, ComputeShader computeShader, 
            int kernelIndex, int3 groups
        ) => cmd.DispatchCompute(computeShader, kernelIndex, groups.x, groups.y, groups.z);
    }
}