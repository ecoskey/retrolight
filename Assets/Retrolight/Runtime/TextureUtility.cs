using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UColor = UnityEngine.Color;

namespace Retrolight.Runtime {
    public static class TextureUtility {
        public static bool IsSrgb => QualitySettings.activeColorSpace == ColorSpace.Gamma;
        
        /*public static TextureDesc Color(Camera camera, string name = "") =>
            Color(Vector2.one, GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, IsSrgb));*/

        private const string 
            DefaultColorTexName = "ColorTex",
            DefaultDepthTexName = "DepthTex";
        
        public static TextureDesc ColorTex(string name = DefaultColorTexName) => ColorTex(Vector2.one, name);

        public static TextureDesc ColorTex(Vector2 scale, string name = DefaultColorTexName) => 
            ColorTex(scale, GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, IsSrgb), name);

        public static TextureDesc ColorTex(
            Vector2 scale,
            GraphicsFormat format, 
            string name = DefaultColorTexName
        ) => new TextureDesc(Vector2.one) {
            colorFormat = format,
            depthBufferBits = DepthBits.None,
            clearBuffer = true,
            clearColor = UColor.clear,
            enableRandomWrite = false, 
            filterMode = FilterMode.Point,
            msaaSamples = MSAASamples.None,
            useDynamicScale = false,
            name = name
        };

        public static TextureDesc DepthTex(string name = DefaultDepthTexName) => DepthTex(Vector2.one, name);

        public static TextureDesc DepthTex(Vector2 scale, string name = DefaultDepthTexName) =>
            new TextureDesc(scale) {
                colorFormat = GraphicsFormat.None,
                depthBufferBits = DepthBits.Depth32,
                clearBuffer = true,
                clearColor = UColor.black,
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = name
            };
    }
}