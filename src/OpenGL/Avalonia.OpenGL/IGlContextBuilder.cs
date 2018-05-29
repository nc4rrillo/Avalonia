using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.OpenGL
{
    public interface IGlContextBuilder
    {
        IGlContext Build(IEnumerable<object> surfaces);
    }
}
