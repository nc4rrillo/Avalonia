using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.OpenGL
{
    public enum GlRequest
    {
        /// <summary>
        /// Tries GL and if that fails tries GLES
        /// </summary>
        Auto,

        /// <summary>
        /// Tries GL
        /// </summary>
        Gl,

        /// <summary>
        /// Tries GLES
        /// </summary>
        Gles
    }

    public enum GlVersion
    {
        Latest,
        Specific
    }
}
