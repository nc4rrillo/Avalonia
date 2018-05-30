// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.Platform;
using SkiaSharp;

namespace Avalonia.Skia
{
    /// <summary>
    /// Skia platform render interface.
    /// </summary>
    public class PlatformRenderInterface : IPlatformRenderInterface
    {
        private readonly IGlContextBuilder _contextBuilder;
        private IGlContext _context;

        /// <summary>
        /// Create new Skia platform render interface using optional Gpu render backend.
        /// </summary>
        /// <param name="contextBuilder">The GL context builder. Null if Gpu acceleration is turned off</param>
        public PlatformRenderInterface(IGlContextBuilder contextBuilder)
        {
            _contextBuilder = contextBuilder;
        }

        /// <summary>
        /// True, if Gpu backend is present.
        /// </summary>
        private bool HasGpuSupport => _contextBuilder != null;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            foreach (var surface in surfaces)
            {
                if (HasGpuSupport)
                {
                    _context = _contextBuilder.Build(surfaces);
                    _context.MakeCurrent();

                    var grContext = GRContext.Create(GRBackend.OpenGL, GRGlInterface.AssembleInterface((o, name) => _context.GetProcAddress(name)), GRContextOptions.Default);
                    return new WindowRenderTarget(_context, grContext);
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

        /// <inheritdoc />
        public IWriteableBitmapImpl CreateWriteableBitmap(int width, int height, PixelFormat? format = null)
        {
            return new WriteableBitmapImpl(width, height, format);
        }
    }
}