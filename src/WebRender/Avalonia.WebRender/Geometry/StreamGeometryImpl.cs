using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.WebRender.Geometry
{
    public class StreamGeometryImpl : IStreamGeometryImpl, ITransformedGeometryImpl
    {
        public Rect Bounds => new Rect();

        public IGeometryImpl SourceGeometry => throw new NotImplementedException();
        public Matrix Transform => throw new NotImplementedException();

        public IStreamGeometryImpl Clone()
        {
            return new StreamGeometryImpl();
        }

        public bool FillContains(Point point)
        {
            return false;
        }

        public Rect GetRenderBounds(Pen pen)
        {
            return new Rect();
        }

        public IGeometryImpl Intersect(IGeometryImpl geometry)
        {
            throw new NotImplementedException();
        }

        public IStreamGeometryContextImpl Open()
        {
            return new StreamGeometryContextImpl();
        }

        public bool StrokeContains(Pen pen, Point point)
        {
            return false;
        }

        public ITransformedGeometryImpl WithTransform(Matrix transform)
        {
            return this;
        }
    }
}
