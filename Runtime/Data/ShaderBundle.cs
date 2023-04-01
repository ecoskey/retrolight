using UnityEngine;

namespace Data {
    //[CreateAssetMenu(fileName = "Retrolight Shader Bundle", menuName = "Retrolight/Shader Bundle", order = 0)]
    public class ShaderBundle : ScriptableObject {
        [SerializeField] private ComputeShader lightCullingShader;
        [SerializeField] private ComputeShader lightingShader;

        [SerializeField] private Shader blitShader;
        [SerializeField] private Shader blitWithDepthShader;
        [SerializeField] private Shader blurShader;
        [SerializeField] private Shader bloomCombineShader;


        [SerializeField] private Shader colorCorrectionShader;

        public ComputeShader LightCullingShader => lightCullingShader;
        public ComputeShader LightingShader => lightingShader;
        
        public Shader BlitShader => blitShader;
        public Shader BlitWithDepthShader => blitWithDepthShader;
        public Shader BlurShader => blurShader;
        public Shader ColorCorrectionShader => colorCorrectionShader;
        public Shader BloomCombineShader => bloomCombineShader;
    }
}