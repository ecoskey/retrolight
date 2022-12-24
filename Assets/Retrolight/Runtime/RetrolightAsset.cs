using Retrolight.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    [CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Pipeline", order = 0)]
    public class RetrolightAsset : RenderPipelineAsset {
        [SerializeField, Range(1, 8)] private int pixelRatio = 4;

        protected override RenderPipeline CreatePipeline() {
            var shaderBundle = Resources.Load<ShaderBundle>("Retrolight/Retrolight Shader Bundle");
            return new Retrolight(shaderBundle, pixelRatio);
        }
    }
}