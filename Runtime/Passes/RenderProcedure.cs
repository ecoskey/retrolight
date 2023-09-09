using System;
using Retrolight.Data;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Retrolight.Passes {
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