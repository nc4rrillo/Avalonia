using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.OpenGL
{
    /// <summary>
    /// Framebuffer descriptor info.
    /// </summary>
    public struct FramebufferParameters
    {
        /// <summary>
        /// Handle to the framebuffer.
        /// </summary>
        public IntPtr FramebufferHandle;

        /// <summary>
        /// Sample count used by framebuffer.
        /// </summary>
        public int SampleCount;

        /// <summary>
        /// Stencil bits used by framebuffer.
        /// </summary>
        public int StencilBits;
    }
}
