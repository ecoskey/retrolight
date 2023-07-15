using UnityEngine;
using UnityEngine.Rendering;

namespace Data {
    public readonly struct FrameData {
        public readonly Camera Camera;
        public readonly CullingResults Cull;
        public readonly ViewportParams ViewportParams;
        public readonly bool UseHDR;

        public FrameData(Camera camera, CullingResults cull, ViewportParams viewportParams, bool allowHDR) {
            Camera = camera;
            Cull = cull;
            ViewportParams = viewportParams;
            UseHDR = allowHDR && camera.allowHDR;
        }
    }
}