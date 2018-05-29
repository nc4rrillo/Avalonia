using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Win32.EGL
{
    public class EglSurface : IGlSurface, IDisposable
    {
        public IntPtr SurfaceHandle { get; }
        public IPlatformHandle PlatformHandle { get; }

        public EglSurface(IntPtr surfaceHandle, IPlatformHandle platformHandle)
        {
            SurfaceHandle = surfaceHandle;
            PlatformHandle = platformHandle;
        }

        public (int width, int height) GetSize()
        {
            UnmanagedMethods.GetClientRect(PlatformHandle.Handle, out UnmanagedMethods.RECT clientSize);

            return (clientSize.right - clientSize.left, clientSize.bottom - clientSize.top);
        }

        public void Dispose()
        {
        }
    }
}
