using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Win32.EGL
{
    public class EglContext : IGlContext
    {
        private IntPtr context = IntPtr.Zero;
        private IntPtr display = IntPtr.Zero;

        public IGlSurface Surface { get; private set; }

        public EglContext(IntPtr display, IGlSurface surface, IntPtr context)
        {
            this.display = display;
            this.Surface = surface;
            this.context = context;
        }

        public IntPtr GetProcAddress(string functionName)
        {
            return Natives.GetProcAddress(functionName);
        }

        public bool IsCurrentContext()
        {
            return Natives.GetCurrentContext() == context;
        }

        public void MakeCurrent()
        {
            Natives.MakeCurrent(this.display, this.Surface.SurfaceHandle, this.Surface.SurfaceHandle, this.context);
        }

        public void SwapBuffers()
        {
            Natives.SwapBuffers(this.display, this.Surface.SurfaceHandle);
        }

        public void Dispose()
        {
            this.Surface.Dispose();
        }
    }
}
