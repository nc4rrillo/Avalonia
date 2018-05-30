using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.OpenGL
{
    /// <summary>
    /// GL renderable surface.
    /// </summary>
    public interface IGlSurface : IDisposable
    {
        IntPtr SurfaceHandle { get; }

        /// <summary>
        /// Get size of a surface.
        /// </summary>
        /// <returns>Size of a surface.</returns> 
        (int width, int height) GetSize();

        /// <summary>
        /// Get dpi of a surface. 
        /// </summary>
        /// <returns>Dpi of a surface</returns> 
        (int x, int y) GetDpi();
    }
}
