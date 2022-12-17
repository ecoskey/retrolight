using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Runtime {
    [CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Pipeline", order = 0)]
    public class RetrolightPipelineAsset : RenderPipelineAsset {
        [SerializeField, Range(1, 16)] private uint pixelScale;

        protected override RenderPipeline CreatePipeline() {
            var shaderBundle = Resources.Load<ShaderBundle>("Retrolight Shader Bundle");
            return new Retrolight(shaderBundle, pixelScale);
        }
    }
}