using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.WebRender.Geometry;
using Avalonia.WebRender.Imaging;
using Avalonia.WebRender.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace Avalonia
{
    public static class WebRenderApplicationExtensions
    {
        public static T UseWebRender<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            builder.UseRenderingSubsystem(WebRender.WebRenderPlatform.Initialize, "WebRender");
            return builder;
        }
    }
}

namespace Avalonia.WebRender
{
    public class WebRenderPlatform : IPlatformRenderInterface
    {
        public static void Initialize()
        {
            var renderInterface = new WebRenderPlatform();
            AvaloniaLocator.CurrentMutable
                .Bind<Func<IRenderRoot, IRenderer>>().ToConstant<Func<IRenderRoot, IRenderer>>((root) => new DisplayListRenderer(root))
                .Bind<IPlatformRenderInterface>().ToConstant(renderInterface);
        }

        public IFormattedTextImpl CreateFormattedText(string text, Typeface typeface, TextAlignment textAlignment, TextWrapping wrapping, Size constraint, IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            return new FormattedTextImpl(text, typeface, textAlignment, wrapping, constraint, spans);
        }

        public IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            var contextBuilder = AvaloniaLocator.Current.GetService<Func<GlRequest, IGlContextBuilder>>()(GlRequest.Auto);
            return new WindowRenderTarget(contextBuilder.Build(surfaces));
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(int width, int height, double dpiX, double dpiY)
        {
            throw new NotImplementedException();
        }

        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return new StreamGeometryImpl();
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(int width, int height, PixelFormat? format = null)
        {
            throw new NotImplementedException();
        }

        public IBitmapImpl LoadBitmap(string fileName)
        {
            return new BitmapImpl();
        }

        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new BitmapImpl();
        }

        public IBitmapImpl LoadBitmap(PixelFormat format, IntPtr data, int width, int height, int stride)
        {
            return new BitmapImpl();
        }
    }
}
