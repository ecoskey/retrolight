using UnityEngine;
using UnityEngine.Rendering;

namespace Data {
    public struct ShadowedLight {
        public readonly int VisibleLightIndex;
        public readonly Matrix4x4 MatrixV;
        public readonly Matrix4x4 MatrixP;
        public readonly ShadowSplitData SplitData;

        public ShadowedLight(
            int visibleLightIndex, Matrix4x4 matrixV, Matrix4x4 matrixP,
            ShadowSplitData splitData
        ) {
            VisibleLightIndex = visibleLightIndex;
            MatrixV = matrixV;
            MatrixP = matrixP;
            SplitData = splitData;
        }
    }
}