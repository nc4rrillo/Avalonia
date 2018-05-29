using Avalonia.OpenGL;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalonia.Win32.EGL
{
    public enum EglApi
    {
        Gl,
        Gles
    }

    public class EglContextBuilder : IGlContextBuilder
    {
        private GlRequest request;
        private GlVersion version;

        private IntPtr display = IntPtr.Zero;
        private List<string> extensions = new List<string>();
        private int eglMajor;
        private int eglMinor;
        private IntPtr configId;
        private EglApi eglApi;

        public EglContextBuilder(GlRequest request)
        {
            this.display = Natives.GetDisplay((IntPtr)Natives.DEFAULT_DISPLAY);
            if (display == IntPtr.Zero)
                throw new GlContextException("Could not create EGL display object!");

            if (!Natives.Initialize(display, out eglMajor, out eglMinor))
                throw new GlContextException("Could not initialize EGL!");

            // EGL >= 1.4 lets us query extensions
            if (this.eglMajor >= 1 && this.eglMinor >= 4)
            {
                this.extensions.AddRange(Natives.QueryString(display, Natives.EXTENSIONS).Split(' '));
            }

            this.eglApi = EglContextBuilder.BindAPI(this.eglMajor, this.eglMinor, request);
            this.configId = EglContextBuilder.ChooseFbConfig(this.display, this.eglMajor, this.eglMinor, this.eglApi);
        }

        public IGlContext Build(IEnumerable<object> surfaces)
        {
            var hwnd = surfaces.FirstOrDefault(s => s is IPlatformHandle) as PlatformHandle;
            if (hwnd == null)
                throw new InvalidOperationException("Could not find a HWND to build a surface from!");

            // TODO: More surface attributes?
            var attributes = new int[]
            {
                Natives.NONE
            };

            var surface = Natives.CreateWindowSurface(this.display, this.configId, hwnd.Handle, attributes);
            
            // TODO: Make the GL version configurable
            return new EglContext(this.display, new EglSurface(surface, hwnd), this.CreateContext(3, 0));
        }

        private static EglApi BindAPI(int eglMajor, int eglMinor, GlRequest request)
        {
            // EGL defaults to OPENGL_ES for eglBindAPI.
            var eglApi = EglApi.Gles;

            //If we have EGL >= 1.4, we can try desktop GL if our GlRequest is set to Auto
            if (eglMajor >= 1 && eglMinor >= 4)
            {
                if (request == GlRequest.Auto)
                {
                    if (Natives.BindAPI(Natives.OPENGL_API))
                    {
                        eglApi = EglApi.Gl;
                    }
                    else if (Natives.BindAPI(Natives.OPENGL_ES_API))
                    {
                        eglApi = EglApi.Gles;
                    }
                    else
                    {
                        throw new GlContextException("Could not find a suitable OpenGL API to bind to!");
                    }
                }
                else if (request == GlRequest.Gl)
                {
                    if (!Natives.BindAPI(Natives.OPENGL_API))
                    {
                        throw new GlContextException("Could not find a suitable OpenGL API to bind to!");
                    }
                }
            }

            return eglApi;
        }

        private static IntPtr ChooseFbConfig(IntPtr display, int major, int minor, EglApi api)
        {
            var attributes = new List<int>();

            if (major >= 1 && minor >= 2)
            {
                attributes.Add(Natives.COLOR_BUFFER_TYPE);
                attributes.Add(Natives.RGB_BUFFER);
            }

            attributes.Add(Natives.SURFACE_TYPE);
            attributes.Add(Natives.WINDOW_BIT);

            // Add the Renderable type and Conformant attributes based on the selected API
            if (api == EglApi.Gles)
            {
                if (major <= 1 && minor < 3)
                {
                    throw new GlContextException("No available pixel format!");
                }

                attributes.Add(Natives.RENDERABLE_TYPE);

                // TODO: ES2 Bit if we request EGL version 2.0
                attributes.Add(Natives.OPENGL_ES3_BIT);

                attributes.Add(Natives.CONFORMANT);
                attributes.Add(Natives.OPENGL_ES3_BIT);
            }
            else
            {
                if (major <= 1 && minor < 3)
                {
                    throw new GlContextException("No available pixel format!");
                }

                attributes.Add(Natives.RENDERABLE_TYPE);
                attributes.Add(Natives.OPENGL_BIT);
                attributes.Add(Natives.CONFORMANT);
                attributes.Add(Natives.OPENGL_BIT);
            }

            // Use HW acceleration
            attributes.Add(Natives.CONFIG_CAVEAT);
            attributes.Add(Natives.NONE);

            attributes.Add(Natives.RED_SIZE);
            attributes.Add(8);

            attributes.Add(Natives.GREEN_SIZE);
            attributes.Add(8);

            attributes.Add(Natives.BLUE_SIZE);
            attributes.Add(8);

            attributes.Add(Natives.ALPHA_SIZE);
            attributes.Add(8);

            attributes.Add(Natives.DEPTH_SIZE);
            attributes.Add(24);

            attributes.Add(Natives.STENCIL_SIZE);
            attributes.Add(8);

            attributes.Add(Natives.NONE);

            var configIds = new IntPtr[1];
            if (!Natives.ChooseConfig(display, attributes.ToArray(), configIds, 1, out var numConfigs))
            {
                throw new GlContextException("eglChooseConfig failed!");
            }

            if (numConfigs == 0)
            {
                throw new GlContextException("No available pixel format!");
            }

            return configIds[0];
        }

        private IntPtr CreateContext(int contextMajor, int contextMinor)
        {
            var attributes = new List<int>();

            // EGL >= 1.5 or implementations with EGL_KHR_create_context can request a minor version as well
            if ((this.eglMajor >= 1 && this.eglMinor >= 5) || this.extensions.Contains("EGL_KHR_create_context"))
            {
                attributes.Add(Natives.CONTEXT_CLIENT_VERSION);
                attributes.Add(contextMajor);
                attributes.Add(Natives.CONTEXT_MINOR_VERSION);
                attributes.Add(contextMinor);

                // TODO: Robustness
            }
            else if ((this.eglMajor >= 1 && this.eglMinor >= 3) && this.eglApi == EglApi.Gles)
            {
                attributes.Add(Natives.CONTEXT_CLIENT_VERSION);
                attributes.Add(contextMajor);
            }

            attributes.Add(Natives.NONE);

            var context = Natives.CreateContext(this.display, this.configId, (IntPtr)Natives.NO_CONTEXT, attributes.ToArray());
            if (context == IntPtr.Zero)
                throw new GlContextException($"Could not create OpenGL {contextMajor}.{contextMinor} context!");

            return context;
        }
    }
}
