using System;
using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Passes {
    public abstract class RenderProcedure : IDisposable {
        /// <summary>
        /// NOTE: ONLY USE THIS TO INITIALIZE RENDER PASSES
        /// </summary>
        private readonly Retrolight pipeline;

        protected RenderProcedure(Retrolight pipeline) { this.pipeline = pipeline; }
        
        public abstract void Run(RenderGraph renderGraph, FrameData frameData);
        
        public abstract void Dispose();
    }
}