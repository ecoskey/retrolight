using UnityEngine;

namespace Retrolight.Runtime {
    [CreateAssetMenu(fileName = "Shader Bundle", menuName = "Shader Bundle", order = 0)]
    public class ShaderBundle : ScriptableObject {
        [SerializeField] private ComputeShader lightCullingShader;
        [SerializeField] private ComputeShader lightingShader;

        [SerializeField] private Shader blitShader;
        [SerializeField] private Shader blitWithDepthShader;

        public ComputeShader LightCullingShader => lightCullingShader;
        public ComputeShader LightingShader => lightingShader;

        public Shader BlitShader => blitShader;
        public Shader BlitWithDepthShader => blitWithDepthShader;
    }
}