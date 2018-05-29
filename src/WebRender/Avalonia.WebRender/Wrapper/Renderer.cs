using Avalonia.OpenGL;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    public delegate IntPtr GetGlFunctionAddressCallback(IntPtr symbol);
    public delegate void WakeupCallback(ulong window_id);
    public delegate void ExternalEventCallback(ulong window_id, uint eventId);
    public delegate void NewFrameReadyCallback(ulong window_id);
    public delegate void NopFrameDoneCallback(ulong window_id);

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderNotifier
    {
        public WakeupCallback OnWakeup;
        public NewFrameReadyCallback OnNewFrame;
        public NopFrameDoneCallback OnNopFrame;
        public ExternalEventCallback OnExternalEvent;
    }

    public class Renderer : WrObject
    {
        [DllImport("webrender_bindings")]
        public extern static bool wr_window_new(
            ulong windowId,
            RenderNotifier renderNotifier,
            uint width,
            uint height,
            GetGlFunctionAddressCallback getProcAddress,
            out IntPtr document,
            out IntPtr renderer,
            out int maxTextureSize
        );

        [DllImport("webrender_bindings.dll")]
        public extern static bool wr_renderer_render(
            IntPtr renderer,
            uint width,
            uint height
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_renderer_update(
            IntPtr renderer
        );

        public Document RootDocument { get; protected set; }
        private RenderNotifier notifier;
        private IGlContext context;

        public Renderer(IGlContext context, ulong windowId)
        {
            this.context = context;
            this.context.MakeCurrent();

            this.notifier = new RenderNotifier
            {
                OnNewFrame = (wid) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (!this.context.IsCurrentContext())
                        {
                            this.context.MakeCurrent();
                        }

                        var (w, h) = this.context.Surface.GetSize();

                        this.Update();
                        this.Render((uint)w, (uint)h);
                        this.context.SwapBuffers();
                    });
                },
                OnNopFrame = (wid) =>
                {

                },
                OnWakeup = (wid) =>
                {

                },
                OnExternalEvent = (wid, evt) =>
                {

                }
            };

            var (width, height) = this.context.Surface.GetSize();
            Renderer.wr_window_new(
                windowId, notifier, 
                (uint)width, (uint)height,
                (sym) => context.GetProcAddress(Marshal.PtrToStringAnsi(sym)), 
                out var documentPtr, out var rendererPtr, out var maxTextureSize
            );

            this.RootDocument = new Document(documentPtr);
            this.NativePtr = rendererPtr;
        }

        public void Update()
        {
            Renderer.wr_renderer_update(this.NativePtr);
        }

        public void Render(uint width, uint height)
        {
            Renderer.wr_renderer_render(this.NativePtr, width, height);
        }

        public override void Dispose()
        {
            this.RootDocument.Dispose();
        }
    }
}
