using UnityEngine;

namespace Retrolight.Passes {
    public abstract class RenderProcedureAsset : ScriptableObject {
        public abstract RenderProcedure GetRenderProcedure(Retrolight pipeline);
    }
}