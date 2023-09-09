using UnityEngine;

namespace Retrolight.Data {
    public struct ShadowedLight {
        public readonly int VisibleLightIndex;
        public readonly Matrix4x4 MatrixV;
        public readonly Matrix4x4 MatrixP;

        public ShadowedLight(int visibleLightIndex, Matrix4x4 matrixV, Matrix4x4 matrixP) {
            VisibleLightIndex = visibleLightIndex;
            MatrixV = matrixV;
            MatrixP = matrixP;
        }
    }
}