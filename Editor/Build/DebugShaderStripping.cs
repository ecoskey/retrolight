using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Retrolight.Editor.Build {
    public class DebugShaderStripping : IShaderVariantStripper, IComputeShaderVariantStripper {
        private static readonly GlobalKeyword globalDebugKeyword = GlobalKeyword.Create("EDITOR_DEBUG");
       
        bool IVariantStripper<Shader, ShaderSnippetData>.active => true;
        bool IVariantStripper<ComputeShader, string>.active => true;

        public bool CanRemoveVariant(
            Shader shader,
            ShaderSnippetData shaderVariant,
            ShaderCompilerData shaderCompilerData
        ) {
            var localDebugKeyword = new LocalKeyword(shader, "EDITOR_DEBUG");
            var keywordSet = shaderCompilerData.shaderKeywordSet;
            return keywordSet.IsEnabled(globalDebugKeyword) || keywordSet.IsEnabled(localDebugKeyword);
        }

        public bool CanRemoveVariant(
            ComputeShader shader, 
            string shaderVariant, 
            ShaderCompilerData shaderCompilerData
        ) {
            var localDebugKeyword = new LocalKeyword(shader, "EDITOR_DEBUG");
            var keywordSet = shaderCompilerData.shaderKeywordSet;
            return keywordSet.IsEnabled(globalDebugKeyword) || keywordSet.IsEnabled(localDebugKeyword);
        }
        
    }
}