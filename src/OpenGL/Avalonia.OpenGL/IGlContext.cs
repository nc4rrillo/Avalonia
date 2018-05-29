using System;

namespace Avalonia.OpenGL
{
    /// <summary>
    /// GL Context
    /// </summary>
    public interface IGlContext : IDisposable
    {
        /// <summary>
        /// Sets the GL Surface for a Context
        /// </summary>
        IGlSurface Surface { get; }

        void SwapBuffers();

        /// <summary>
        /// Makes this GL Context current
        /// </summary>
        void MakeCurrent();

        IntPtr GetProcAddress(string functionName);

        /// <returns>True if this Context is the current one. False otherwise.</returns>
        bool IsCurrentContext();

        void RecreateSurface();
    }
}
