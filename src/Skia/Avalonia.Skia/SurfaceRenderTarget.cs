// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.IO;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Skia.Helpers;
using SkiaSharp;

namespace Avalonia.Skia
{
    /// <summary>
    /// Skia render target that writes to a surface. Can be Gpu accelerated.
    /// </summary>
    public class SurfaceRenderTarget : IRenderTargetBitmapImpl, IDrawableBitmapImpl
    {
        private readonly Vector _dpi;
        private readonly IGlContext _renderContext;
        private readonly GRContext _grContext;
        private readonly SKSurface _surface;
        private readonly SKCanvas _canvas;
        private readonly bool _disableLcdRendering;

        /// <summary>
        /// Create new surface render target.
        /// </summary>
        /// <param name="createInfo">Create info.</param>
        public SurfaceRenderTarget(CreateInfo createInfo)
        {
            PixelWidth = createInfo.Width;
            PixelHeight = createInfo.Height;
            _dpi = createInfo.Dpi;
            _renderContext = createInfo.RenderContext;
            _disableLcdRendering = createInfo.DisableTextLcdRendering;
            _renderContext = createInfo.RenderContext;
            _grContext = createInfo.GrContext;

            _surface = CreateSurface(PixelWidth, PixelHeight, createInfo.Format, _renderContext, createInfo.GrContext);

            _canvas = _surface?.Canvas;

            if (_surface == null || _canvas == null)
            {
                throw new InvalidOperationException("Failed to create Skia render target surface");
            }
        }

        /// <summary>
        /// Create backing Skia surface.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="format">Format.</param>
        /// <param name="renderContext">Render context.</param>
        /// <returns></returns>
        private static SKSurface CreateSurface(int width, int height, PixelFormat? format, IGlContext renderContext, GRContext grContext)
        {
            var imageInfo = MakeImageInfo(width, height, format);

            return renderContext != null
                ? SKSurface.Create(grContext, false, imageInfo)
                : SKSurface.Create(imageInfo);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _canvas.Dispose();
            _surface.Dispose();
        }

        /// <inheritdoc />
        public IDrawingContextImpl CreateDrawingContext(IVisualBrushRenderer visualBrushRenderer)
        {
            _canvas.RestoreToCount(-1);
            _canvas.ResetMatrix();

            _renderContext?.MakeCurrent();

            var createInfo = new DrawingContextImpl.CreateInfo
            {
                Canvas = _canvas,
                Dpi = _dpi,
                VisualBrushRenderer = visualBrushRenderer,
                RenderContext = _renderContext,
                GrContext = _grContext,
                DisableTextLcdRendering = _disableLcdRendering
            };

            return new DrawingContextImpl(createInfo);
        }

        /// <inheritdoc />
        public int PixelWidth { get; }

        /// <inheritdoc />
        public int PixelHeight { get; }

        /// <inheritdoc />
        public void Save(string fileName)
        {
            using (var image = SnapshotImage())
            {
                ImageSavingHelper.SaveImage(image, fileName);
            }
        }

        /// <inheritdoc />
        public void Save(Stream stream)
        {
            using (var image = SnapshotImage())
            {
                ImageSavingHelper.SaveImage(image, stream);
            }
        }

        /// <inheritdoc />
        public void Draw(DrawingContextImpl context, SKRect sourceRect, SKRect destRect, SKPaint paint)
        {
            using (var image = SnapshotImage())
            {
                context.Canvas.DrawImage(image, sourceRect, destRect, paint);
            }
        }

        /// <summary>
        /// Create Skia image snapshot from a surface.
        /// </summary>
        /// <returns>Image snapshot.</returns>
        public SKImage SnapshotImage()
        {
            return _surface.Snapshot();
        }

        /// <summary>
        /// Create image info for given parameters.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="format">Format.</param>
        /// <returns></returns>
        private static SKImageInfo MakeImageInfo(int width, int height, PixelFormat? format)
        {
            var colorType = PixelFormatHelper.ResolveColorType(format);

            return new SKImageInfo(width, height, colorType, SKAlphaType.Premul);
        }

        /// <summary>
        /// Create info of a surface render target.
        /// </summary>
        public struct CreateInfo
        {
            /// <summary>
            /// Width of a render target.
            /// </summary>
            public int Width;

            /// <summary>
            /// Height of a render target.
            /// </summary>
            public int Height;

            /// <summary>
            /// Dpi used when rendering to a surface.
            /// </summary>
            public Vector Dpi;

            /// <summary>
            /// Pixel format of a render target.
            /// </summary>
            public PixelFormat? Format;

            /// <summary>
            /// Optional render context, when passed surface will use Gpu acceleration.
            /// </summary>
            public IGlContext RenderContext;

            public GRContext GrContext;

            /// <summary>
            /// Render text without Lcd rendering.
            /// </summary>
            public bool DisableTextLcdRendering;
        }
    }
}