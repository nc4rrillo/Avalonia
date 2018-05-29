using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalonia.WebRender
{
    public class DisplayListRenderer : IRenderer, IDisposable, IVisualBrushRenderer
    {
        public bool DrawFps { get; set; } = false;
        public bool DrawDirtyRects { get; set; } = false;

        private IRenderRoot root = null;
        private IRenderTarget renderTarget = null;

        public DisplayListRenderer(IRenderRoot root)
        {
            this.root = root;
        }

        public void AddDirty(IVisual visual)
        {
            if (this.root != null)
            {
                var transform = visual.TransformToVisual(this.root);

                if (transform.HasValue)
                {
                    var bounds = new Rect(visual.Bounds.Size).TransformToAABB(transform.Value);
                    this.root?.Invalidate(visual.Bounds);
                }
            }
        }

        public void Dispose()
        {
            this.renderTarget?.Dispose();
        }

        static IEnumerable<IVisual> HitTest(
           IVisual visual,
           Point p,
           Func<IVisual, bool> filter)
        {
            Contract.Requires<ArgumentNullException>(visual != null);

            if (filter?.Invoke(visual) != false)
            {
                bool containsPoint = visual.TransformedBounds?.Contains(p) == true;

                if ((containsPoint || !visual.ClipToBounds) && visual.VisualChildren.Count > 0)
                {
                    foreach (var child in visual.VisualChildren.SortByZIndex())
                    {
                        foreach (var result in HitTest(child, p, filter))
                        {
                            yield return result;
                        }
                    }
                }

                if (containsPoint)
                {
                    yield return visual;
                }
            }
        }

        public IEnumerable<IVisual> HitTest(Point p, IVisual root, Func<IVisual, bool> filter)
        {
            return HitTest(root, p, filter);
        }

        public void Paint(Rect rect)
        {
            if (this.renderTarget == null)
                this.renderTarget = this.root.CreateRenderTarget();

            using (var context = new DrawingContext(this.renderTarget.CreateDrawingContext(this)))
            {
                this.Render(context, this.root, this.root.Bounds);
            }
        }

        private static void ClearTransformedBounds(IVisual visual)
        {
            foreach (var e in visual.GetSelfAndVisualDescendants())
            {
                visual.TransformedBounds = null;
            }
        }

        private static Rect GetTransformedBounds(IVisual visual)
        {
            if (visual.RenderTransform == null)
            {
                return visual.Bounds;
            }
            else
            {
                var origin = visual.RenderTransformOrigin.ToPixels(new Size(visual.Bounds.Width, visual.Bounds.Height));
                var offset = Matrix.CreateTranslation(visual.Bounds.Position + origin);
                var m = (-offset) * visual.RenderTransform.Value * (offset);
                return visual.Bounds.TransformToAABB(m);
            }
        }

        private static UInt64 monotonicAnimationId = 0;

        private void Render(DrawingContext context, IVisual visual, Rect clipRect)
        {
            if (visual.IsVisible && visual.Opacity > 0)
            {
                // Create the transform for the SC
                var transform = Matrix.CreateTranslation(visual.Bounds.Position);
                var bounds = new Rect(visual.Bounds.Size);
                var opacity = visual.Opacity;
                var clipToBounds = visual.ClipToBounds;

                var renderTransform = Matrix.Identity;
                if (visual.RenderTransform != null)
                {
                    var origin = visual.RenderTransformOrigin.ToPixels(new Size(visual.Bounds.Width, visual.Bounds.Height));
                    var offset = Matrix.CreateTranslation(origin);
                    renderTransform = (-offset) * visual.RenderTransform.Value * (offset);
                }

                transform = renderTransform * transform;

                using (context.PushOpacity(opacity))
                using (((DrawingContextImpl)context.PlatformImpl).PushReferenceFrame(transform, bounds, 0))
                using (clipToBounds ? context.PushClip(bounds) : default(DrawingContext.PushedState))
                using (visual.Clip != null ? context.PushGeometryClip(visual.Clip) : default(DrawingContext.PushedState))
                using (visual.OpacityMask != null ? context.PushOpacityMask(visual.OpacityMask, bounds) : default(DrawingContext.PushedState))
                using (context.PushTransformContainer())
                {
                    visual.Render(context);

#pragma warning disable 0618
                    var transformed = new TransformedBounds(bounds, new Rect(), context.CurrentContainerTransform);
#pragma warning restore 0618

                    visual.TransformedBounds = transformed;

                    foreach (var child in visual.VisualChildren.OrderBy(x => x, ZIndexComparer.Instance))
                    {
                        var childBounds = GetTransformedBounds(child);
                        if (!child.ClipToBounds || clipRect.Intersects(childBounds))
                        {
                            var childClipRect = clipRect.Translate(-childBounds.Position);
                            Render(context, child, childClipRect);
                        }
                        else
                        {
                            ClearTransformedBounds(child);
                        }
                    }
                }
            }

            if (!visual.IsVisible)
            {
                ClearTransformedBounds(visual);
            }
        }

        public void Resized(Size size)
        {
            // We can probably create a txn here to do SetWindowParameters 
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public Size GetRenderTargetSize(IVisualBrush brush)
        {
            (brush.Visual as IVisualBrushInitialize)?.EnsureInitialized();
            return brush.Visual?.Bounds.Size ?? Size.Empty;
        }
        
        public void RenderVisualBrush(IDrawingContextImpl context, IVisualBrush brush)
        {
            var visual = brush.Visual;
            Render(new DrawingContext(context), visual, visual.Bounds);
        }
    }
}
