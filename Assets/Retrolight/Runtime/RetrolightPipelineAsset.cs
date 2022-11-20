using Retrolight.Runtime;
using UnityEngine.Rendering;

public class RetrolightPipelineAsset : RenderPipelineAsset {
    protected override RenderPipeline CreatePipeline() { return new RetrolightPipeline(); }
}
