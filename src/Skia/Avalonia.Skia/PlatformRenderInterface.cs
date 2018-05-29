using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.Platform;
using SkiaSharp;

namespace Avalonia.Skia
{
    public partial class PlatformRenderInterface : IPlatformRenderInterface
    {
        public IBitmapImpl CreateBitmap(int width, int height)
        {
            return CreateRenderTargetBitmap(width, height, 96, 96);
        }

        public IFormattedTextImpl CreateFormattedText(
            string text,
            Typeface typeface,
            TextAlignment textAlignment,
            TextWrapping wrapping,
            Size constraint,
            IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            return new FormattedTextImpl(text, typeface, textAlignment, wrapping, constraint, spans);
        }

        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return new StreamGeometryImpl();
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new ImmutableBitmap(stream);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return LoadBitmap(stream);
            }
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(PixelFormat format, IntPtr data, int width, int height, int stride)
        {
            return new ImmutableBitmap(width, height, stride, format, data);
        }

        /// <inheritdoc />
        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(
            int width,
            int height,
            double dpiX,
            double dpiY)
        {
            if (width < 1)
            {
                throw new ArgumentException("Width can't be less than 1", nameof(width));
            }

            if (height < 1)
            {
                throw new ArgumentException("Height can't be less than 1", nameof(height));
            }

            var dpi = new Vector(dpiX, dpiY);

            /*
            var createInfo = new SurfaceRenderTarget.CreateInfo
            {
                Width = width,
                Height = height,
                Dpi = dpi,
                RenderContext = HasGpuSupport ? _renderBackend.CreateOffscreenRenderContext() : null,
                DisableTextLcdRendering = !HasGpuSupport
            };

            return new SurfaceRenderTarget(createInfo);*/

            throw new NotImplementedException();
        }

        public virtual IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            foreach (var surface in surfaces)
            {
                if (true)
                {
                    var contextBuilder = AvaloniaLocator.Current.GetService<Func<GlRequest, IGlContextBuilder>>();
                    var context = contextBuilder(GlRequest.Auto).Build(surfaces);
                    context.MakeCurrent();

                    GL.Initialize((name) => context.GetProcAddress(name));
                    var grContext = GRContext.Create(GRBackend.OpenGL, GRGlInterface.AssembleInterface((o, name) => context.GetProcAddress(name)), GRContextOptions.Default);
                    return new WindowRenderTarget(context, grContext);
                }
                else
                {
                    if (surface is IFramebufferPlatformSurface framebufferSurface)
                    {
                        return new FramebufferRenderTarget(framebufferSurface);
                    }
                }
            }

            throw new NotSupportedException("Don't know how to create a Skia render target from any of provided surfaces");
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(int width, int height, PixelFormat? format = null)
        {
            return new WriteableBitmapImpl(width, height, format);
        }
    }
}
