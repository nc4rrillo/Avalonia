using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.OpenGL
{
    public class GlContextException : Exception
    {
        public GlContextException(string message) : base(message) { }
    }
}
