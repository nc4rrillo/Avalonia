using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    public class Document : WrObject
    {
        [DllImport("webrender_bindings")]
        public static extern void wr_api_create_document(IntPtr rootDocument, out IntPtr documentPtr);

        [DllImport("webrender_bindings")]
        public static extern void wr_api_delete_document(IntPtr document);

        public Document(IntPtr nativePtr) : base(nativePtr) { }
        public Document(Document rootDocument)
        {
            Document.wr_api_create_document(rootDocument.NativePtr, out var nativePtr);
            this.NativePtr = nativePtr;
        }

        public override void Dispose()
        {
            Document.wr_api_delete_document(this.NativePtr);
        }
    }
}
