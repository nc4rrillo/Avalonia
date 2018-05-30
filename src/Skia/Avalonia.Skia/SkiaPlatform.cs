// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using Avalonia.Gpu;
using Avalonia.Logging;
using Avalonia.OpenGL;
using Avalonia.Platform;

namespace Avalonia.Skia
{
    /// <summary>
    /// Skia backend types.
    /// </summary>
    public enum RenderBackendType
    {
        /// <summary>
        /// Cpu based backend.
        /// </summary>
        Cpu,

        /// <summary>
        /// Gpu based backend.
        /// </summary>
        Gpu
    }

    /// <summary>
    /// Skia platform initializer.
    /// </summary>
    public static class SkiaPlatform
    {
        /// <summary>
        /// Initialize Skia platform.
        /// </summary>
        /// <param name="preferredBackendType">Preferred backend type - will fallback to cpu if platform has not support for it.</param>
        public static void Initialize(RenderBackendType preferredBackendType = RenderBackendType.Cpu)
        {
            Logger.Information(LogArea.Visual, null, "SkiaRuntime initializing with backend: {backendType}", preferredBackendType);

            IGlContextBuilder contextBuilder = null;
            if (preferredBackendType == RenderBackendType.Gpu)
            {
                var contextBuilderFactory = AvaloniaLocator.Current.GetService<Func<GlRequest, IGlContextBuilder>>();
                if (contextBuilder != null)
                {
                    try
                    {
                        contextBuilder = contextBuilderFactory(new GlRequest { Api = GlApi.Auto, Version = GlVersion.Latest });
                    }
                    catch (Exception e)
                    {
                        Logger.Warning(LogArea.Visual, null, "Failed to start EGL platform due to {e}", e);
                    }
                }
            }

            var renderInterface = new PlatformRenderInterface(contextBuilder);

            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformRenderInterface>().ToConstant(renderInterface);
        }

        /// <summary>
        /// Default DPI.
        /// </summary>
        public static Vector DefaultDpi => new Vector(96.0f, 96.0f);
    }
}
