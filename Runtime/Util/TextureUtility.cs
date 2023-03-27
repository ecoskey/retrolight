using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Util {
    public static class TextureUtility {
        public static bool IsSrgb => QualitySettings.activeColorSpace == ColorSpace.Gamma;

        public const string
            DefaultColorTexName = "ColorTex",
            DefaultDepthTexName = "DepthTex";

        public static TextureDesc ColorTex(string name = DefaultColorTexName) => ColorTex(Vector2.one, name);

        public static TextureDesc ColorTex(Vector2 scale, string name = DefaultColorTexName) =>
            ColorTex(scale, GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, IsSrgb), name);

        public static TextureDesc ColorTex(
            Vector2 scale, GraphicsFormat format,
            string name = DefaultColorTexName
        ) => new TextureDesc(scale) {
            colorFormat = format,
            depthBufferBits = DepthBits.None,
            clearBuffer = true,
            clearColor = Color.clear,
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
                enableRandomWrite = false,
                filterMode = FilterMode.Point,
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                name = name
            };
    }
}