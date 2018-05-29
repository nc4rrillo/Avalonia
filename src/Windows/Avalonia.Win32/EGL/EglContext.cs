using Avalonia.Logging;
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
        private IntPtr configId = IntPtr.Zero;

        public IGlSurface Surface { get; private set; }

        public EglContext(IntPtr display, IntPtr configId, IGlSurface surface, IntPtr context)
        {
            this.display = display;
            this.configId = configId;
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

        public void RecreateSurface()
        {
            Natives.MakeCurrent(this.display, (IntPtr)Natives.NO_SURFACE, (IntPtr)Natives.NO_SURFACE, this.context);
            if (!Natives.DestroySurface(this.display, this.Surface.SurfaceHandle))
            {
                var error = Natives.GetError();
                Logger.Warning(LogArea.Visual, this, "Failed to destroy EGL surface with handle {handle}. Error: {error}", this.Surface.SurfaceHandle, error);
            }

            // TODO: More surface attributes?
            var attributes = new int[]
            {
                Natives.NONE
            };

            var newSurface = Natives.CreateWindowSurface(this.display, this.configId, ((EglSurface)this.Surface).PlatformHandle.Handle, attributes);
            if (newSurface == IntPtr.Zero)
            {
                var error = Natives.GetError();
                Logger.Warning(LogArea.Visual, this, "Failed to create EGL surface. Error: {error}", error);
            }

            this.Surface = new EglSurface(newSurface, ((EglSurface)this.Surface).PlatformHandle);
            Natives.MakeCurrent(this.display, this.Surface.SurfaceHandle, this.Surface.SurfaceHandle, this.context);
        }
    }
}
