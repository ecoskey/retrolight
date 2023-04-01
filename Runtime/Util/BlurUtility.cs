using System;
using Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Util {
    public class BlurUtility : IDisposable {
        private static readonly Shader blurShader;
        private readonly Material blurMaterial;
        private readonly LocalKeyword blurKeyword;

        private static readonly LocalKeyword keyGaussian9, keyBox3, keyBox5, keyBox7;
        public enum BlurType { Gaussian9, Box3, Box5, Box7 }

        private static readonly int
            blurSourceId = Shader.PropertyToID("_BlurSource"),
            texelSizeId = Shader.PropertyToID("_TexelSize");

        static BlurUtility() {
            blurShader = Shader.Find("Hidden/Retrolight/Blur");
            keyGaussian9 = new LocalKeyword(blurShader, "GAUSSIAN_9");
            keyBox3 = new LocalKeyword(blurShader, "BOX_3");
            keyBox5 = new LocalKeyword(blurShader, "BOX_5");
            keyBox7 = new LocalKeyword(blurShader, "BOX_7");
        }

        public BlurUtility(BlurType blurType) {
            blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
            blurKeyword = blurType switch {
                BlurType.Gaussian9 => keyGaussian9,
                BlurType.Box3 => keyBox3,
                BlurType.Box5 => keyBox5,
                BlurType.Box7 => keyBox7,
                _ => keyBox3
            };
            blurMaterial.EnableKeyword(blurKeyword);
        }
        
        public void Blur(CommandBuffer cmd, RTHandle source, RTHandle to, RTHandle temp) {
            var blurProps = new MaterialPropertyBlock();
            blurProps.Clear();
            
            var texelSize = (Vector2) Vector2Int.one / source.GetScaledSize();

            blurProps.SetTexture(blurSourceId, source);
            blurProps.SetFloat(texelSizeId, texelSize.x);
            CoreUtils.DrawFullScreen(cmd, blurMaterial, temp, blurProps, 0);
            
            blurProps.SetTexture(blurSourceId, temp);
            blurProps.SetFloat(texelSizeId, texelSize.y);
            CoreUtils.DrawFullScreen(cmd, blurMaterial, to, blurProps, 1);
        }
        
        public void BlurInPlace(CommandBuffer cmd, RTHandle source, RTHandle temp) {
            var blurProps = new MaterialPropertyBlock();
            blurProps.Clear();
            
            var texelSize = (Vector2) Vector2Int.one / source.GetScaledSize();

            blurProps.SetTexture(blurSourceId, source);
            blurProps.SetFloat(texelSizeId, texelSize.x);
            CoreUtils.DrawFullScreen(cmd, blurMaterial, temp, blurProps, 0);
            
            blurProps.SetTexture(blurSourceId, temp);
            blurProps.SetFloat(texelSizeId, texelSize.y);
            CoreUtils.DrawFullScreen(cmd, blurMaterial, source, blurProps, 1);
        }

        public void Dispose() {
            CoreUtils.Destroy(blurMaterial);
            blurMaterial.DisableKeyword(blurKeyword);
        }
    }
}