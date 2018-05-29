using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.WebRender.Text;
using Avalonia.WebRender.Utils;
using Avalonia.WebRender.Wrapper;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace Avalonia.WebRender
{
    public class DrawingContextImpl : IDrawingContextImpl
    {
        // This is ignored by the WR DrawingContext and we instead rely on a PushReferenceFrame that our custom Renderer calls for us.
        public Matrix Transform { get; set; }

        private Document document;
        private Transaction transaction;
        private DisplayListBuilder builder;
        private LayoutSize viewportSize;

        public DrawingContextImpl(Document document, LayoutSize layoutSize)
        {
            this.document = document;
            this.viewportSize = layoutSize;

            // TODO: Support multiple Pipelines if it makes sense?
            this.builder = new DisplayListBuilder(new PipelineId(0, 0), this.viewportSize);
            this.transaction = new Transaction(this.document);
        }

        public void Clear(Color color)
        {
        }

        public IRenderTargetBitmapImpl CreateLayer(Size size)
        {
            throw new NotImplementedException("CreateLayer not supported using WebRender!");
        }

        public void Dispose()
        {
            var builtDisplayList = this.builder.Build();
            this.transaction.SetDisplayList(
                new PipelineId(0, 0),
                0,
                new ColorF { R = 0.0f, G = 0.0f, B = 0.0f, A = 1.0f },
                builtDisplayList.Item2,
                builtDisplayList
            );

            this.transaction.SetWindowParameters((uint)this.viewportSize.width, (uint)this.viewportSize.height);
            this.transaction.GenerateFrame();
            this.transaction.Dispose();
        }

        public void DrawGeometry(IBrush brush, Pen pen, IGeometryImpl geometry)
        {
        }

        public void DrawImage(IRef<IBitmapImpl> source, double opacity, Rect sourceRect, Rect destRect)
        {
        }

        public void DrawImage(IRef<IBitmapImpl> source, IBrush opacityMask, Rect opacityMaskRect, Rect destRect)
        {
        }

        public void DrawLine(Pen pen, Point p1, Point p2)
        {
        }

        public void DrawRectangle(Pen pen, Rect rect, float cornerRadius = 0)
        {
        }

        public void DrawText(IBrush foreground, Point origin, IFormattedTextImpl text)
        {
            ColorF color = new ColorF { R = 0.0f, G = 0.0f, B = 0.0f, A = 1.0f };
            if (foreground is SolidColorBrush)
            {
                var scb = foreground as SolidColorBrush;
                color = scb.Color.ToColorF();
            }
            else if (foreground is ImmutableSolidColorBrush)
            {
                var iscb = (ImmutableSolidColorBrush)foreground;
                color = iscb.Color.ToColorF();
            }

            var rect = new Rect(origin, text.Size);
            (text as FormattedTextImpl).BuildFontKey(this.document, rect.Position);

            var layoutRect = new LayoutRect(
                new LayoutPoint(
                    (float)rect.X,
                    (float)rect.Y
                ),
                new LayoutSize(
                    (float)rect.Width,
                    (float)rect.Height
                )
            );

            var font_key_instance = (text as FormattedTextImpl).FontInstanceKey;
            var indices = (text as FormattedTextImpl).GlyphInstances;
            this.builder.PushText(
                font_key_instance,
                indices ?? new GlyphInstance[0],
                indices != null ? indices.Length : 0,
                layoutRect,
                color
            );
        }

        public void FillRectangle(IBrush brush, Rect rect, float cornerRadius = 0)
        {
            var layoutRect = rect.ToLayoutRect();
            if (brush is SolidColorBrush || brush is ImmutableSolidColorBrush)
            {
                var scb = brush as SolidColorBrush;
                if (scb == null)
                {
                    var iscb = (ImmutableSolidColorBrush)brush;
                    this.builder.PushRect(
                         layoutRect,
                         iscb.Color.ToColorF()
                     );
                }
                else
                {
                    this.builder.PushRect(
                        layoutRect,
                        scb.Color.ToColorF()
                    );
                }
            }
        }

        public IDisposable PushReferenceFrame(Matrix transform, Rect bounds, ulong animationId)
        {
            // Avalonia uses a 2x3 Matrix in column major order, WR expects a 4x4 Matrix in row major order. 
            // Cheat a bit by converting to an SkMatrix to swap the ordering from column major.
            var xform = transform.ToSKMatrix();
            var animationOpacity = new WrAnimationProperty { AnimationId = animationId };
            var currentOpacity = (float)this.opacity;
            builder.PushReferenceFrame(
                new LayoutRect(
                    new LayoutPoint((float)bounds.X, (float)bounds.Y),
                    new LayoutSize((float)bounds.Width, (float)bounds.Height)
                ),
                new LayoutTransform(
                    (float)xform.Values[0],
                    (float)xform.Values[1],
                    (float)xform.Values[2], 0.0f,
                    (float)xform.Values[3],
                    (float)xform.Values[4],
                    (float)xform.Values[5], 0.0f,
                    (float)xform.Values[6],
                    (float)xform.Values[7],
                    (float)xform.Values[8], 0.0f,
                    xform.TransX, xform.TransY, 0.0f, 1.0f),
                ref animationOpacity,
                currentOpacity
            );

            return Disposable.Create(() => this.PopReferenceFrame());
        }

        // TODO: PopReferenceFrame
        public void PopReferenceFrame()
        {
            builder.PopReferenceFrame();
        }

        public void PopClip()
        {
            builder.PopClip();
        }

        public void PopGeometryClip()
        {
        }

        public void PopOpacity()
        {
            this.opacity = this.opacityStack.Pop();
        }

        public void PopOpacityMask()
        {
        }

        public void PushClip(Rect clip)
        {
            var clipId = builder.DefineClip(clip.ToLayoutRect());
            builder.PushClip(clipId);
        }

        public void PushGeometryClip(IGeometryImpl clip)
        {
        }

        private double opacity;
        private Stack<double> opacityStack = new Stack<double>();

        public void PushOpacity(double opacity)
        {
            this.opacityStack.Push(this.opacity);
            this.opacity = opacity;
        }

        public void PushOpacityMask(IBrush mask, Rect bounds)
        {
        }
    }
}
