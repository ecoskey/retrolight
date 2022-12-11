using UnityEngine;

namespace Retrolight.Runtime {
    [CreateAssetMenu(fileName = "Compute Bundle", menuName = "Compute Bundle", order = 0)]
    public class ShaderBundle : ScriptableObject {
        [SerializeField] private ComputeShader lightCullShader;
        [SerializeField] private ComputeShader lightingShader;

        [SerializeField] private Shader blitShader;
        [SerializeField] private Shader blitWithDepthShader;

        public ComputeShader LightCullShader => LightCullShader;
        public ComputeShader LightingShader => lightingShader;

        public Shader BlitShader => blitShader;
        public Shader BlitWithDepthShader => blitWithDepthShader;
    }
}