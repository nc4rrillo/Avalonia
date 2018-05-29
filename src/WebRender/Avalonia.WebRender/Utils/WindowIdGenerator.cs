using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.WebRender.Utils
{
    public static class WindowIdGenerator
    {
        private static ulong currentWindowId;
        public static ulong NextWindowId() => currentWindowId++;
    }
}
