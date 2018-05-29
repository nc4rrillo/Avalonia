using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.OpenGL;
namespace Avalonia.Win32.WGL
{
    public class WglContext : IGlContext
    {
        public IGlSurface Surface => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IntPtr GetProcAddress(string functionName)
        {
            throw new NotImplementedException();
        }

        public bool IsCurrentContext()
        {
            throw new NotImplementedException();
        }

        public void MakeCurrent()
        {
            throw new NotImplementedException();
        }

        public void RecreateSurface()
        {
            throw new NotImplementedException();
        }

        public void SwapBuffers()
        {
            throw new NotImplementedException();
        }
    }
}
