using Data;
using Passes;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Retrolight Settings", order = 0)]
public class RetrolightAsset : RenderPipelineAsset<Retrolight> {
    [SerializeField, Range(1, 8)] private int pixelRatio = 4;
    [SerializeField] private bool allowPostFx;
    [SerializeField] private bool allowHDR;

    [SerializeField] private ShadowSettings shadowSettings;
    
    protected override RenderPipeline CreatePipeline() {
        return new Retrolight(
            pixelRatio, allowPostFx, allowHDR, shadowSettings.Validate(), 
            pipeline => new DefaultRenderProcedure(pipeline)
        );
    }
}
