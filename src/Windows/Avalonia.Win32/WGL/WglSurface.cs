using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Win32.WGL
{
    public class WglSurface : IGlSurface
    {
        public IntPtr SurfaceHandle => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public (int width, int height) GetSize()
        {
            throw new NotImplementedException();
        }
    }
}
