using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Retrolight.Util {
    public static class RenderUtils {
        private static class ShaderIds {
            public static int SourceTex0 = Shader.PropertyToID("SrcTex0");
            public static int SourceTex0_ST = Shader.PropertyToID("SrcTex0_ST");
            public static int SourceTex0Res = Shader.PropertyToID("SrcTex0Res");
            public static int SourceTex1 = Shader.PropertyToID("SrcTex1");
            public static int SourceTex1_ST = Shader.PropertyToID("SrcTex1_ST");
            public static int SourceTex1Res = Shader.PropertyToID("SrcTex1Res");
            public static int OutputTex = Shader.PropertyToID("OutTex");
            public static int OutputTex_ST = Shader.PropertyToID("OutTex_ST");
            public static int OutputTexRes = Shader.PropertyToID("OutTexRes");
        }

        private struct ComputeContext {
            public ComputeShader shader;
            public int kernel;
            public int tileSizeX, tileSizeY;

            public ComputeContext(ComputeShader shader, int kernel, int tileSizeX, int tileSizeY) {
                this.shader = shader;
                this.kernel = kernel;
                this.tileSizeX = tileSizeX;
                this.tileSizeY = tileSizeY;
            }
        }

        public class InPlaceFullscreenPassData<R> {
            public R renderer;
            public TextureHandle tex;
            public bool assumeDestFullscreen;
        }

        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            Material mat, TextureHandle tex, bool assumeDestFullscreen = false
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out InPlaceFullscreenPassData<Material> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = mat;
            passData.tex = builder.UseColorBuffer(builder.ReadTexture(tex), 0);
            passData.assumeDestFullscreen = assumeDestFullscreen;
            builder.SetRenderFunc<InPlaceFullscreenPassData<Material>>(RenderInPlaceFullscreen_Fragment);
        }
        
        private static void RenderInPlaceFullscreen_Fragment(
            InPlaceFullscreenPassData<Material> passData, RenderGraphContext ctx
        ) {
            Material renderer = passData.renderer;
            Vector2 texScale = TextureUtils.GetTexScale(passData.tex);
            Vector4 texRes = TextureUtils.GetTexRes(passData.tex);
            
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture(ShaderIds.SourceTex0, passData.tex);
            props.SetVector(ShaderIds.SourceTex0_ST, texScale);
            props.SetVector(ShaderIds.SourceTex0Res, texRes);
            props.SetVector(ShaderIds.OutputTex_ST, texScale);
            props.SetVector(ShaderIds.OutputTexRes, texRes);
            
            if (!passData.assumeDestFullscreen) ctx.cmd.SetViewport(new Rect(Vector2.zero, texScale));
            CoreUtils.DrawFullScreen(ctx.cmd, renderer, props);
            if (!passData.assumeDestFullscreen) ResetViewport(ctx.cmd);
        }

        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            ComputeShader shader, int kernel, int tileSizeX, int tileSizeY,
            TextureHandle tex
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out InPlaceFullscreenPassData<ComputeContext> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = new ComputeContext(shader, kernel, tileSizeX, tileSizeY);
            passData.tex = builder.ReadWriteTexture(tex);
            builder.SetRenderFunc<InPlaceFullscreenPassData<ComputeContext>>(RenderInPlaceFullscreen_Compute);
        }

        private static void RenderInPlaceFullscreen_Compute(
            InPlaceFullscreenPassData<ComputeContext> passData, 
            RenderGraphContext ctx
        ) {
            ComputeContext renderer = passData.renderer;
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.OutputTex, passData.tex);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTex_ST, 
                TextureUtils.GetTexScale(passData.tex)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTexRes, 
                TextureUtils.GetTexRes(passData.tex)
            );
            
            Vector2Int texSize = ((RTHandle) passData.tex).GetScaledSize();
            int tilesX = MathUtils.NextMultipleOf(texSize.x, renderer.tileSizeX);
            int tilesY = MathUtils.NextMultipleOf(texSize.y, renderer.tileSizeY);
            ctx.cmd.DispatchCompute(renderer.shader, renderer.kernel, tilesX, tilesY, 1);
        }
        
        private class OneToOneFullscreenPassData<R> {
            public R renderer;
            public TextureHandle srcTex;
            public TextureHandle outTex;
            public bool assumeDestFullscreen;
        }
        
        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            ComputeShader shader, int kernel, int tileSizeX, int tileSizeY,
            TextureHandle srcTex, TextureHandle outTex
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out OneToOneFullscreenPassData<ComputeContext> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = new ComputeContext(shader, kernel, tileSizeX, tileSizeY);
            passData.srcTex = builder.ReadTexture(srcTex);
            passData.outTex = builder.UseColorBuffer(outTex, 0);
            builder.SetRenderFunc<OneToOneFullscreenPassData<ComputeContext>>(RenderOneToOneFullscreen_Compute);
        }
        
        private static void RenderOneToOneFullscreen_Compute(
            OneToOneFullscreenPassData<ComputeContext> passData, RenderGraphContext ctx
        ) {
            ComputeContext renderer = passData.renderer;
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.SourceTex0, passData.srcTex);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex0_ST, 
                TextureUtils.GetTexScale(passData.srcTex)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex0Res,
                TextureUtils.GetTexRes(passData.srcTex)
            );
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.OutputTex, passData.outTex);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTex_ST, 
                TextureUtils.GetTexScale(passData.outTex)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTexRes, 
                TextureUtils.GetTexRes(passData.outTex)
            );
            
            Vector2Int texSize = ((RTHandle) passData.outTex).GetScaledSize();
            int tilesX = MathUtils.NextMultipleOf(texSize.x, renderer.tileSizeX);
            int tilesY = MathUtils.NextMultipleOf(texSize.y, renderer.tileSizeY);
            ctx.cmd.DispatchCompute(renderer.shader, renderer.kernel, tilesX, tilesY, 1);
        }
        
        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            Material mat, TextureHandle srcTex, TextureHandle outTex, 
            bool assumeDestFullscreen = false
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out OneToOneFullscreenPassData<Material> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = mat;
            passData.srcTex = builder.ReadTexture(srcTex);
            passData.outTex = builder.UseColorBuffer(outTex, 0);
            passData.assumeDestFullscreen = assumeDestFullscreen;
            builder.SetRenderFunc<OneToOneFullscreenPassData<Material>>(RenderOneToOneFullscreen_Fragment);
        }
        
        private static void RenderOneToOneFullscreen_Fragment(
            OneToOneFullscreenPassData<Material> passData, RenderGraphContext ctx
        ) {
            Material renderer = passData.renderer;
            
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture(ShaderIds.SourceTex0, passData.srcTex);
            props.SetVector(ShaderIds.SourceTex0_ST, TextureUtils.GetTexScale(passData.srcTex));
            props.SetVector(ShaderIds.SourceTex0Res, TextureUtils.GetTexRes(passData.srcTex));

            Vector2 outTexScale = TextureUtils.GetTexScale(passData.outTex);
            props.SetVector(ShaderIds.OutputTex_ST, outTexScale);
            props.SetVector(ShaderIds.OutputTexRes, TextureUtils.GetTexRes(passData.outTex));

            if (!passData.assumeDestFullscreen) ctx.cmd.SetViewport(new Rect(Vector2.zero, outTexScale));
            CoreUtils.DrawFullScreen(ctx.cmd, renderer, props);
            if (!passData.assumeDestFullscreen) ResetViewport(ctx.cmd);
        }

        private class TwoToOneFullscreenPassData<R> {
            public R renderer;
            public TextureHandle srcTex0;
            public TextureHandle srcTex1;
            public TextureHandle outTex;
            public bool assumeDestFullscreen;
        }
        
        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            ComputeShader shader, int kernel, int tileSizeX, int tileSizeY,
            TextureHandle srcTex0, TextureHandle srcTex1, TextureHandle outTex,
            bool assumeDestFullscreen = false
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out TwoToOneFullscreenPassData<ComputeContext> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = new ComputeContext(shader, kernel, tileSizeX, tileSizeY);
            passData.srcTex0 = builder.ReadTexture(srcTex0);
            passData.srcTex1 = builder.ReadTexture(srcTex1);
            passData.outTex = builder.UseColorBuffer(outTex, 0);
            passData.assumeDestFullscreen = assumeDestFullscreen;
            builder.SetRenderFunc<TwoToOneFullscreenPassData<ComputeContext>>(RenderTwoToOneFullscreen_Compute);
        }
        
        private static void RenderTwoToOneFullscreen_Compute(
            TwoToOneFullscreenPassData<ComputeContext> passData, RenderGraphContext ctx
        ) {
            ComputeContext renderer = passData.renderer;
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.SourceTex0, passData.srcTex0);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex0_ST, 
                TextureUtils.GetTexScale(passData.srcTex0)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex0Res,
                TextureUtils.GetTexScale(passData.srcTex0)
            );
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.SourceTex1, passData.srcTex1);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex1_ST, 
                TextureUtils.GetTexScale(passData.srcTex1)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.SourceTex1Res,
                TextureUtils.GetTexScale(passData.srcTex1)
            );
            
            ctx.cmd.SetComputeTextureParam(renderer.shader, renderer.kernel, ShaderIds.OutputTex, passData.outTex);
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTex_ST, 
                TextureUtils.GetTexScale(passData.outTex)
            );
            ctx.cmd.SetComputeVectorParam(
                renderer.shader, ShaderIds.OutputTexRes, 
                TextureUtils.GetTexRes(passData.outTex)
            );
            
            Vector2Int texSize = ((RTHandle) passData.outTex).GetScaledSize();
            int tilesX = MathUtils.NextMultipleOf(texSize.x, renderer.tileSizeX);
            int tilesY = MathUtils.NextMultipleOf(texSize.y, renderer.tileSizeY);
            ctx.cmd.DispatchCompute(renderer.shader, renderer.kernel, tilesX, tilesY, 1);
        }
        
        public static void AddFullscreenPass(
            this RenderGraph renderGraph, string passName,
            Material mat, TextureHandle srcTex0, TextureHandle srcTex1, TextureHandle outTex
        ) {
            var builder = renderGraph.AddRenderPass(
                passName, out TwoToOneFullscreenPassData<Material> passData,
                new ProfilingSampler($"{passName} profiler")
            );
            passData.renderer = mat;
            passData.srcTex0 = builder.ReadTexture(srcTex0);
            passData.srcTex1 = builder.ReadTexture(srcTex1);
            passData.outTex = builder.UseColorBuffer(outTex, 0);
            builder.SetRenderFunc<TwoToOneFullscreenPassData<Material>>(RenderTwoToOneFullscreen_Fragment);
            
        }
        
        private static void RenderTwoToOneFullscreen_Fragment(
            TwoToOneFullscreenPassData<Material> passData, RenderGraphContext ctx
        ) {
            Material renderer = passData.renderer;
            
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture(ShaderIds.SourceTex0, passData.srcTex0);
            props.SetVector(ShaderIds.SourceTex0_ST, TextureUtils.GetTexScale(passData.srcTex0));
            props.SetVector(ShaderIds.SourceTex0Res, TextureUtils.GetTexRes(passData.srcTex0));
            props.SetTexture(ShaderIds.SourceTex1, passData.srcTex1);
            props.SetVector(ShaderIds.SourceTex1_ST, TextureUtils.GetTexScale(passData.srcTex1));
            props.SetVector(ShaderIds.SourceTex1Res, TextureUtils.GetTexRes(passData.srcTex1));
            
            //todo: undo this part because setting the viewport should be enough;
            Vector2 outTexScale = TextureUtils.GetTexScale(passData.outTex);
            props.SetVector(ShaderIds.OutputTex_ST, outTexScale);
            props.SetVector(ShaderIds.OutputTexRes, TextureUtils.GetTexRes(passData.outTex));
            
            if (!passData.assumeDestFullscreen) ctx.cmd.SetViewport(new Rect(Vector2.zero, outTexScale));
            CoreUtils.DrawFullScreen(ctx.cmd, renderer, props);
            if (!passData.assumeDestFullscreen) ResetViewport(ctx.cmd);
        }

        private static Rect fullscreenRect = new Rect(0, 0, 1, 1);
        private static void ResetViewport(CommandBuffer cmd) => cmd.SetViewport(fullscreenRect);
    }
}