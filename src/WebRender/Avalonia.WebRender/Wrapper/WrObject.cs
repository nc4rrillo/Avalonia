using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    public abstract class WrObject : IDisposable
    {
        public IntPtr NativePtr { get; protected set; }

        public WrObject() { }
        public WrObject(IntPtr nativePtr)
        {
            this.NativePtr = nativePtr;
        }

        public abstract void Dispose();
    }
}
