using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    /// <summary>
    /// A transaction is a group of operations that should be performed in a single frame
    /// </summary>
    public class Transaction : WrObject
    {
        // TODO: Test out MarshalAs to pass bools as bytes to Rust
        [DllImport("webrender_bindings")]
        private extern static IntPtr wr_transaction_new(IntPtr document, byte isAsync);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_delete(IntPtr txn);

        [DllImport("webrender_bindings")]
        private extern static void wr_api_send_transaction(IntPtr document, IntPtr txn, byte isAsync);

        [DllImport("webrender_bindings")]
        private extern static void wr_api_update_resources(IntPtr document, IntPtr txn);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_generate_frame(IntPtr txn);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_set_window_parameters(IntPtr txn, ref DeviceUintSize size, ref DeviceUintRect documentRect);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_add_image(IntPtr txn, ImageKey imageKey, ref ImageDescriptor imageDescriptor, ref WrVecU8 imageData);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_add_font_descriptor(IntPtr txn, WrFontKey fontKey, IntPtr fontDescriptor);

        [DllImport("webrender_bindings")]
        private extern static WrFontInstanceKey wr_api_generate_font_instance_key(IntPtr txn);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_add_font_instance(IntPtr txn, WrFontInstanceKey fontInstanceKey, WrFontKey fontKey, float fontSize, IntPtr foo, IntPtr bar);

        [DllImport("webrender_bindings")]
        private extern static void wr_transaction_set_display_list(
            IntPtr txn, 
            uint epoch, 
            ColorF background, 
            float width, 
            float height, 
            PipelineId pipeline, 
            LayoutSize contentSize,
            BuiltDisplayListDescriptor descriptor,
            ref WrVecU8 data
        );

        private Document document;
        public Transaction(Document document)
        {
            this.document = document;
            this.NativePtr = Transaction.wr_transaction_new(document.NativePtr, 0);
        }

        public void SetRootPipeline(PipelineId pipeline)
        {

        }

        public void SetDisplayList(
            PipelineId pipeline,
            uint epoch,
            ColorF background,
            LayoutSize viewport,
            (BuiltDisplayListDescriptor, LayoutSize, WrVecU8) builtDisplayList
        )
        {
            Transaction.wr_transaction_set_display_list(
                this.NativePtr,
                epoch,
                background,
                viewport.width,
                viewport.height,
                pipeline,
                builtDisplayList.Item2,
                builtDisplayList.Item1,
                ref builtDisplayList.Item3
            );
        }
        public void GenerateFrame()
        {
            Transaction.wr_transaction_generate_frame(this.NativePtr);
        }

        public void ScrollNodeWithId()
        {
            throw new NotImplementedException();
        }

        public void SetWindowParameters(UInt32 w, UInt32 h)
        {
            var size = new DeviceUintSize(w, h);
            var rect = new DeviceUintRect(new DeviceUintPoint(0, 0), size);
            Transaction.wr_transaction_set_window_parameters(this.NativePtr, ref size, ref rect);
        }

        public void AddImage(ImageKey key, ref ImageDescriptor descriptor, ref WrVecU8 data)
        {
            Transaction.wr_transaction_add_image(
                this.NativePtr,
                key,
                ref descriptor,
                ref data
            );
        }

        public void AddFontDescriptor(IntPtr descriptorPtr, WrFontKey key)
        {
            Transaction.wr_transaction_add_font_descriptor(this.NativePtr, key, descriptorPtr);
        }

        public WrFontInstanceKey GenerateFontInstanceKey()
        {
            return Transaction.wr_api_generate_font_instance_key(this.document.NativePtr);
        }

        public WrFontInstanceKey AddFontInstance(WrFontKey key, float fontSize)
        {
            var fontInstanceKey = this.GenerateFontInstanceKey();
            Transaction.wr_transaction_add_font_instance(this.NativePtr, fontInstanceKey, key, fontSize, IntPtr.Zero, IntPtr.Zero);
            return fontInstanceKey;
        }

        public override void Dispose()
        {
            // Update any resources contained in this transaction (Add/Update/Delete Image/Font)
            Transaction.wr_api_update_resources(this.document.NativePtr, this.NativePtr);

            // Send the transaction to WebRender
            // This includes display lists, scrolls, and animations
            Transaction.wr_api_send_transaction(
                this.document.NativePtr,
                this.NativePtr, 
                0
            );

            // Delete the transaction
            Transaction.wr_transaction_delete(this.NativePtr);
        }
    }
}
