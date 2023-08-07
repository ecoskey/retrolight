using UnityEngine;

namespace Data {
    public class ShaderBundle : ScriptableObject {
        [field: SerializeField] public ComputeShader LightCullingShader { get; private set; }
        [field: SerializeField] public ComputeShader LightingShader { get; private set; }
        [field: SerializeField] public Shader BlitShader { get; private set; }
        [field: SerializeField] public Shader BlitWithDepthShader { get; private set; }
        [field: SerializeField] public ComputeShader BloomShader { get; private set; }
        [field: SerializeField] public ComputeShader ColorCorrectionShader { get; private set; }
        [field: SerializeField] public ComputeShader SsaoShader { get; private set; }

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