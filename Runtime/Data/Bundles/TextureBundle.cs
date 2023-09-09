using UnityEngine;

namespace Retrolight.Data.Bundles {
    public class TextureBundle : ScriptableObject {
        [field: SerializeField] public Texture2D DebugHeatmap { get; private set; }

        private static TextureBundle instance;
        private static bool initted;

        public static TextureBundle Instance {
            get {
                if (!initted) {
                    instance =  UnityEngine.Resources.Load<TextureBundle>("Retrolight/Texture Bundle");
                    initted = true;
                }
                return instance;
            }
        }
    }
}