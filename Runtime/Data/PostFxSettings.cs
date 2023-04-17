using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "PostFX Settings", menuName = "Retrolight/PostFX Settings", order = 1)]
    public class PostFxSettings : ScriptableObject {
        [SerializeField] private BloomSettings bloomSettings;
        public BloomSettings BloomSettings => bloomSettings;

        public PostFxSettings(BloomSettings bloomSettings) {
            this.bloomSettings = bloomSettings;
        }
    }
}