using Retrolight.Data;
using Retrolight.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight {
    [CreateAssetMenu(fileName = "Retrolight Settings", menuName = "Retrolight/Retrolight Settings", order = 0)]
    public class RetrolightAsset : RenderPipelineAsset<Retrolight> {
        [SerializeField, Range(1, 8)] private int pixelRatio = 4;
        [SerializeField] private bool allowPostFx;
        [SerializeField] private bool allowHDR;

        [SerializeField] private ShadowSettings shadowSettings;
        [SerializeField] private Option<GTAOSettings> gtao;


        [Header("Customization")]
        [Tooltip("Use this to completely replace the built in render procedure")]
        [SerializeField] private Option<RenderProcedureAsset> customRenderProcedure;
        [Tooltip("Use this to customize Retrolight's built-in lighting model")]
        [SerializeField] private Option<ComputeShader> customLighting;

        protected override RenderPipeline CreatePipeline() {
            return new Retrolight(
                pixelRatio, allowPostFx, allowHDR, shadowSettings.Validate(), 
                customRenderProcedure.Enabled 
                    ? customRenderProcedure.Value.GetRenderProcedure
                    : pipeline => new DefaultRenderProcedure(pipeline, customLighting, gtao)
            );
        }
    }
}
