using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    public class FrameData {
        public readonly Camera Camera;
        public readonly CullingResults Cull;
        public readonly RTHandleProperties RTHandleProperties;

        public FrameData(Camera camera, CullingResults cull, RTHandleProperties rtHandleProperties) {
            Camera = camera;
            Cull = cull;
            RTHandleProperties = rtHandleProperties;
        }
    }
}