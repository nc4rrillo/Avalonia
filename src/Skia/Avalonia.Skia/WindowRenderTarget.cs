using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace Avalonia.Skia
{
    /// <summary>
    /// Skia render target that renders to a window using Gpu acceleration.
    /// </summary>
    public class WindowRenderTarget : IRenderTarget
    {
        private readonly IGlContext _renderContext;
        private readonly GRContext _grContext;
        private GRBackendRenderTargetDesc _rtDesc;
        private SKSurface _surface;
        private SKCanvas _canvas;
        private Size _surfaceDpi;

        /// <summary>
        /// Create new window render target that will render to given handle using passed render backend.
        /// </summary>
        public WindowRenderTarget(IGlContext renderContext, GRContext grContext)
        {
            _renderContext = renderContext ?? throw new ArgumentNullException(nameof(renderContext));
            _grContext = grContext;

            _rtDesc = CreateInitialRenderTargetDesc();
        }

        private GRBackendRenderTargetDesc CreateInitialRenderTargetDesc()
        {
            _renderContext.MakeCurrent();

            var framebufferDesc = _renderContext.Surface.GetFramebufferParameters();

            var pixelConfig = SKImageInfo.PlatformColorType == SKColorType.Bgra8888
                ? GRPixelConfig.Bgra8888
                : GRPixelConfig.Rgba8888;

            var rtDesc = new GRBackendRenderTargetDesc
            {
                Width = 0,
                Height = 0,
                Config = pixelConfig,
                Origin = GRSurfaceOrigin.BottomLeft,
                SampleCount = 0,
                StencilBits = 8,
                RenderTargetHandle = IntPtr.Zero
            };

            return rtDesc;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _renderContext.MakeCurrent();

            _canvas?.Dispose();
            _surface?.Dispose();
            _renderContext.Dispose();
        }

        /// <summary>
        /// Flush rendering commands and present.
        /// </summary>
        private void Flush()
        {
            _grContext.Flush();
            _renderContext.SwapBuffers();
        }

        /// <summary>
        /// Create surface if needed.
        /// </summary>
        private void CreateSurface()
        {
            var (newWidth, newHeight) = _renderContext.Surface.GetSize();

            var (newDpiWidth, newDpiHeight) =_renderContext.Surface.GetDpi();
            var newDpi = new Size(newDpiWidth, newDpiHeight);

            if (_surface == null || newWidth != _rtDesc.Width || newHeight != _rtDesc.Height || newDpi != _surfaceDpi)
            {
                _renderContext.RecreateSurface();
                _renderContext.MakeCurrent();

                _canvas?.Dispose();
                _surface?.Dispose();

                _rtDesc.Width = newWidth;
                _rtDesc.Height = newHeight;
                _surfaceDpi = newDpi;

                _surface = SKSurface.Create(_grContext, _rtDesc);

                _canvas = _surface?.Canvas;

                if (_surface == null || _canvas == null)
                {
                    throw new InvalidOperationException("Failed to create Skia surface for window render target");
                }
            }
        }

        /// <inheritdoc />
        public IDrawingContextImpl CreateDrawingContext(IVisualBrushRenderer visualBrushRenderer)
        {
            _renderContext.MakeCurrent();

            CreateSurface();

            _canvas.RestoreToCount(-1);
            _canvas.ResetMatrix();

            var createInfo = new DrawingContextImpl.CreateInfo
            {
                Canvas = _canvas,
                Dpi = new Vector(_surfaceDpi.Width, _surfaceDpi.Height),
                VisualBrushRenderer = visualBrushRenderer,
                RenderContext = _renderContext,
                GrContext = _grContext
            };

            return new DrawingContextImpl(createInfo, Disposable.Create(Flush));
        }
    }

}
