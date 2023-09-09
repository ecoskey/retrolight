using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Util {
    public static class TextureUtils {
        public static bool IsSRGB => QualitySettings.activeColorSpace == ColorSpace.Gamma;

        public const string
            DefaultColorTexName = "ColorTex",
            DefaultDepthTexName = "DepthTex";
        
        public static GraphicsFormat GetGraphicsFormat(RenderTextureFormat format) => 
            GraphicsFormatUtility.GetGraphicsFormat(format, IsSRGB);
        public static GraphicsFormat DefaultColorFormat => GetGraphicsFormat(RenderTextureFormat.Default);
        public static GraphicsFormat Packed32Format => GetGraphicsFormat(RenderTextureFormat.ARGB2101010);
        public static GraphicsFormat HdrColorFormat => GetGraphicsFormat(RenderTextureFormat.DefaultHDR);
        public static GraphicsFormat ShadowMapFormat => GetGraphicsFormat(RenderTextureFormat.Shadowmap);

        public static GraphicsFormat PreferHdrFormat(bool hdr) => hdr ? HdrColorFormat : DefaultColorFormat;

        public static TextureDesc ColorTex(string name = DefaultColorTexName) => ColorTex(Vector2.one, name);

        public static TextureDesc ColorTex(Vector2 scale, string name = DefaultColorTexName) =>
            ColorTex(scale, DefaultColorFormat, name);

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

        public static Vector4 GetTexRes(RTHandle tex) {
            Vector2Int srcTexSize = tex.GetScaledSize();
           return new Vector4(
                srcTexSize.x, srcTexSize.y,
                1f / srcTexSize.x, 1f / srcTexSize.y
            );
        }

        public static Vector2 GetTexScale(RTHandle tex) => !tex.useScaling ? Vector2.one : 
            new Vector2(tex.rtHandleProperties.rtHandleScale.x, tex.rtHandleProperties.rtHandleScale.y);
    }
}