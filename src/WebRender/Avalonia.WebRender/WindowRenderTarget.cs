using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.WebRender.Utils;
using Avalonia.WebRender.Wrapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.WebRender
{
    public class WindowRenderTarget : IRenderTarget
    {
        private Renderer renderer;
        private IGlContext context;

        public WindowRenderTarget(IGlContext context)
        {
            this.context = context;
            this.renderer = new Renderer(context, WindowIdGenerator.NextWindowId());
        }

        public IDrawingContextImpl CreateDrawingContext(IVisualBrushRenderer visualBrushRenderer)
        {
            var (w, h) = this.context.Surface.GetSize();
            return new DrawingContextImpl(this.renderer.RootDocument, new LayoutSize { width = w, height = h });
        }

        public void Dispose()
        {
        }
    }
}
