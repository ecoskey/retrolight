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
        
        public static TextureDesc Color(Camera camera, string name = "") =>
            Color(camera, GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, IsSrgb), name);

        public static TextureDesc Color(
            Camera camera,
            GraphicsFormat format, 
            string name = ""
        ) => new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
            colorFormat = format,
            depthBufferBits = DepthBits.None,
            clearBuffer = true,
            clearColor = UColor.black,
            enableRandomWrite = false, 
            filterMode = FilterMode.Point,
            msaaSamples = MSAASamples.None,
            useDynamicScale = false,
            name = name
        };
        
        public static TextureDesc Depth(Camera camera, string name = "") =>
            new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
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