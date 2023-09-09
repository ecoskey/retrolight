using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Data {
    public readonly struct FrameData {
        public readonly Camera Camera;
        public readonly CullingResults Cull;
        public readonly ViewportParams ViewportParams;
        public readonly bool UsePostFx;
        public readonly bool UseHDR;

        public FrameData(Camera camera, CullingResults cull, ViewportParams viewportParams, bool allowPostFx, bool allowHDR) {
            Camera = camera;
            Cull = cull;
            ViewportParams = viewportParams;
            var additionalCameraData = camera.GetRetrolightCameraData();
            UsePostFx = allowPostFx && additionalCameraData.UsePostFX;
            UseHDR = allowHDR && camera.allowHDR;
        }
    }
}