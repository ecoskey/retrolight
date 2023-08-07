using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using float2 = Unity.Mathematics.float2;

namespace Util {
    public static class TextureUtils {
        public static bool IsSrgb => QualitySettings.activeColorSpace == ColorSpace.Gamma;

        public const string
            DefaultColorTexName = "ColorTex",
            DefaultDepthTexName = "DepthTex";

        public static GraphicsFormat DefaultColorFormat =>
            GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, IsSrgb);
        public static GraphicsFormat Packed32Format =>
            GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.ARGB2101010, IsSrgb);
        public static GraphicsFormat HdrColorFormat =>
            GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.DefaultHDR, IsSrgb);
        public static GraphicsFormat ShadowMapFormat =>
            GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Shadowmap, IsSrgb);

        public static GraphicsFormat PreferHdrFormat(bool hdr) => hdr ? HdrColorFormat : DefaultColorFormat;

        public static TextureDesc ColorTex(string name = DefaultColorTexName) => ColorTex(float2(1), name);

        public static TextureDesc ColorTex(float2 scale, string name = DefaultColorTexName) =>
            ColorTex(scale, DefaultColorFormat, name);

        public static TextureDesc ColorTex(
            float2 scale, GraphicsFormat format,
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

        public static TextureDesc DepthTex(string name = DefaultDepthTexName) => DepthTex(float2(1), name);

        public static TextureDesc DepthTex(float2 scale, string name = DefaultDepthTexName) =>
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