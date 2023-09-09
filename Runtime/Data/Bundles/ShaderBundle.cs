using UnityEngine;

namespace Retrolight.Data.Bundles {
    [CreateAssetMenu(fileName = "Shader Bundle")]
    public class ShaderBundle : ScriptableObject {
        [field: Header("Lighting")]
        [field: SerializeField] public ComputeShader GTAOShader { get; private set; }
        [field: SerializeField] public ComputeShader LightCullingShader { get; private set; }
        [field: SerializeField] public ComputeShader LightingShader { get; private set; }
        
        [field: Header("Post Processing")]
        [field: SerializeField] public ComputeShader BloomShader { get; private set; }
        [field: SerializeField] public ComputeShader ColorCorrectionShader { get; private set; }
        [field: SerializeField] public ComputeShader CompositingShader { get; private set; }
        
        [field: Header("Util")]
        [field: SerializeField] public Shader BlitShader { get; private set; }
        [field: SerializeField] public Shader BlitWithDepthShader { get; private set; }
        [field: SerializeField] public Shader UpscaleShader { get; private set; }
        [field: SerializeField] public ComputeShader HilbertShader { get; private set; }

        private static ShaderBundle instance;
        private static bool initted;

        public static ShaderBundle Instance {
            get {
                if (!initted) {
                    instance =  UnityEngine.Resources.Load<ShaderBundle>("Retrolight/Shader Bundle");
                    initted = true;
                }
                return instance;
            }
        }
    }
}