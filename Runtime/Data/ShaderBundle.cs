using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "Retrolight Shader Bundle", menuName = "Retrolight/Shader Bundle", order = 0)]
    public class ShaderBundle : ScriptableObject {
        [SerializeField] private ComputeShader lightCullingShader;
        [SerializeField] private ComputeShader lightingShader;

        [SerializeField] private Shader blitShader;
        [SerializeField] private Shader blitWithDepthShader;
        [SerializeField] private ComputeShader bloomShader;
        [SerializeField] private ComputeShader colorCorrectionShader;

        public ComputeShader LightCullingShader => lightCullingShader;
        public ComputeShader LightingShader => lightingShader;
        
        public Shader BlitShader => blitShader;
        public Shader BlitWithDepthShader => blitWithDepthShader;
        
        public ComputeShader BloomShader => bloomShader;
        public ComputeShader ColorCorrectionShader => colorCorrectionShader;
    }
}