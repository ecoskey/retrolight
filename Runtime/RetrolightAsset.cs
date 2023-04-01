using Data;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Retrolight Settings", order = 0)]
public class RetrolightAsset : RenderPipelineAsset {
    [SerializeField, Range(1, 8)] private int pixelRatio = 4;
    [SerializeField] private bool usePostFx;
    [SerializeField] private PostFxSettings postFxSettings;
    
    protected override RenderPipeline CreatePipeline() {
        var shaderBundle = Resources.Load<ShaderBundle>("Retrolight/Retrolight Shader Bundle");
        return new Retrolight(shaderBundle, pixelRatio, usePostFx, postFxSettings);
    }
}
