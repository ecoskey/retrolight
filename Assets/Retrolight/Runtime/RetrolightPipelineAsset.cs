using Retrolight.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Pipeline", order = 0)]
public class RetrolightPipelineAsset : RenderPipelineAsset {
    public ComputeShader shader;
    
    [SerializeField, Range(1, 16)] private uint pixelScale;
    protected override RenderPipeline CreatePipeline() { return new RetrolightPipeline(pixelScale, shader); }
}
