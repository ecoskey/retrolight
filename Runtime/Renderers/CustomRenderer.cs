using System.Collections.Generic;
using Retrolight.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Renderers { 
    public class CustomRenderer : MonoBehaviour {
        //[SerializeField] private Material[] materials;
        
        //todo: figure out what the other fields do
        [Header("Lighting")] 
        [SerializeField] protected ShadowCastingMode castShadows;
        [SerializeField] protected bool receiveShadows;
        [SerializeField] protected bool contributeGlobalIllumination;
        [SerializeField] protected bool receiveGlobalIllumination;

        [Header("Lightmapping")] 
        [SerializeField, Min(0f)] protected float scaleInLightmap;
        [SerializeField] protected bool stitchSeams;
        //[SerializeField] private LightMapP TODO: lightmap parameter selection

        [Header("Probes")] 
        [SerializeField] protected LightProbeUsage lightProbes;
        [SerializeField] protected ReflectionProbeUsage reflectionProbes;
        [SerializeField] protected Transform anchorOverride;

        [Header("Additional Settings")] 
        [SerializeField] protected MotionVectorGenerationMode motionVectors;
        [SerializeField] protected bool dynamicOcclusion;
        [SerializeField] protected LayerMask renderingLayerMask = -0x7FFFFFFF;
        
        protected RenderParams GetRenderParams(Material material) => 
            new RenderParams(material) {
                layer = gameObject.layer,
                
                shadowCastingMode = castShadows,
                receiveShadows = receiveShadows,

                lightProbeUsage = lightProbes,
                reflectionProbeUsage = reflectionProbes,
                
                motionVectorMode = motionVectors,
                renderingLayerMask = (uint) renderingLayerMask.value
            };

        // ReSharper disable once ParameterHidesMember
        /*private Matrix4x4 GetSkewedModelMatrix(Matrix4x4 m, Camera camera) {
            //world to camera or camera to world? I think the second one
            Vector3 cameraForward = -camera.cameraToWorldMatrix.GetColumn(2); // negate? wait is this fine actually?
            Vector3 skewAxis = new Vector3(cameraForward.x, -skewStrength * cameraForward.y, cameraForward.z).normalized;

            // this isn't right, wah wah
            
            m.m00 = skewAxis.x;
            m.m10 = skewAxis.y;

            return m;
        }*/
    }
}