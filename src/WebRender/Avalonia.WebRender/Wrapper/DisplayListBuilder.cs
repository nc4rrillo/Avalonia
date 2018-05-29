using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    public enum MixBlendMode : UInt32
    {
        Normal = 0,
        Multiply,
        Screen,
        Overlay,
        Darken,
        Lighten,
        ColorDodge,
        ColorBurn,
        HardLight,
        SoftLight,
        Difference,
        Exclusion,
        Hue,
        Saturation,
        Color,
        Luminosity,
        Sentinel
    }

    public enum TransformStyle : UInt32
    {
        Flat = 0,
        Preserve3D,
        Sentinel
    }

    public enum GlyphRasterSpaceTag : UInt32
    {
        Local,
        Screen,
        Sentinel
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphRasterSpace
    {
        GlyphRasterSpaceTag tag;
        float local;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WrVecU8
    {
        public IntPtr data;
        public UIntPtr length;
        public UIntPtr capacity;

        public WrVecU8(IntPtr data, UIntPtr length, UIntPtr capacity)
        {
            this.data = data;
            this.length = length;
            this.capacity = capacity;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorF
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BuiltDisplayListDescriptor
    {
        public ulong BuilderStartTime;
        public ulong BuilderEndTime;
        public ulong SendStartTime;
        public uint TotalClipIds;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BorderWidths
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    public enum BorderStyle : UInt32
    {
        None = 0,
        Solid,
        Double,
        Dotted,
        Dashed,
        Hidden,
        Groove,
        Ridge,
        Inset,
        Outset
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BorderSide
    {
        public ColorF Color;
        public BorderStyle Style;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BorderRadius
    {
        public LayoutSize TopLeft;
        public LayoutSize TopRight;
        public LayoutSize BottomLeft;
        public LayoutSize BottomRight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageKey
    {
        public UInt32 Namespace;
        public UInt32 ResourceId;
    }

    public enum ImageFormat : UInt32
    {
        R8 = 1,
        BGRA8 = 3,
        RGBAF32 = 4,
        RG8 = 5,
    }

    public enum ImageRendering : UInt32
    {
        Auto = 0,
        CrispEdges = 1,
        Pixelated = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDescriptor
    {
        public ImageFormat Format;
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 Stride;
        public byte IsOpaque { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WrOpacityProperty
    {
        public ulong AnimationId;
        public float Opacity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WrAnimationProperty
    {
        public ulong AnimationType;
        public ulong AnimationId;
    }

    public enum FontRenderMode : UInt32
    {
        Mono = 0,
        Alpha,
        Subpixel,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphOptions
    {
        public FontRenderMode RenderMode;
        public UInt32 FontInstanceFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WrFontInstanceKey
    {
        public UInt32 Namespace;
        public UInt32 Key;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WrFontKey
    {
        public UInt32 Namespace;
        public UInt32 Key;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphInstance
    {
        public int Index;
        public LayoutPoint Point;
    }

    public class DisplayListBuilder : WrObject
    {
        [DllImport("webrender_bindings")]
        public extern static IntPtr wr_state_new(PipelineId pipeline, LayoutSize size, byte isAsync);

        [DllImport("webrender_bindings")]
        public extern static void wr_state_delete(IntPtr statePtr);

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_push_border(
            IntPtr state_handle,
            LayoutRect rect,
            LayoutRect clip,
            byte is_backface_visible,
            BorderWidths widths,
            BorderSide top,
            BorderSide right,
            BorderSide bottom,
            BorderSide left,
            BorderRadius radius
        );

        [DllImport("webrender_bindings.dll")]
        public static extern void wr_dp_push_text(
            IntPtr builder,
            LayoutRect bounds,
            byte isBackfaceVisible,
            ColorF color,
            WrFontInstanceKey font_key,
            GlyphInstance[] glyphs,
            int glyphCount,
            ref GlyphOptions glyphOptions
        );

        [DllImport("webrender_bindings.dll")]
        private extern static bool wr_api_finalize_builder(
            IntPtr state_handle,
            out LayoutSize layout_size,
            out BuiltDisplayListDescriptor bdld,
            out WrVecU8 dl_data
        );

        [DllImport("webrender_bindings.dll")]
        private extern static bool wr_dp_push_rect(
            IntPtr state_handle,
            LayoutRect rect,
            byte isBackfaceVisible,
            ColorF color
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_push_stacking_context(
            IntPtr state,
            LayoutRect bounds,
            IntPtr clipNodeId,
            ref WrAnimationProperty animationProperty,
            ref float opacity,
            ref LayoutTransform transform,
            TransformStyle transformStyle,
            IntPtr perspective,
            MixBlendMode mixBlendMode,
            IntPtr filters,
            int filterCount,
            byte isBackfaceVisible,
            GlyphRasterSpace rasterSpace,
            out byte isReferenceFrame,
            out uint referenceFrameId
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_pop_stacking_context(
            IntPtr builder
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_push_clip(
           IntPtr builder,
           uint clipId
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_pop_clip(
            IntPtr builder
        );

        [DllImport("webrender_bindings.dll")]
        public extern static uint wr_dp_define_clip(
            IntPtr buiilder, 
            IntPtr parentClipId, 
            LayoutRect clipRect, 
            IntPtr complexClip, 
            uint complexClipCount, 
            IntPtr foo
        );

        [DllImport("webrender_bindings.dll")]
        public extern static void wr_dp_push_text(
            IntPtr builder,
            LayoutRect rect,
            uint z, 
            ColorF color,
            WrFontInstanceKey fontInstanceKey,
            GlyphInstance[] glyphs,
            uint glyphCount,
            ref GlyphOptions glyphOptions
        );

        private bool isBuilt = false;
        public DisplayListBuilder(PipelineId pipeline, LayoutSize size)
        {
            this.NativePtr = DisplayListBuilder.wr_state_new(pipeline, size, 0);
        }

        public void PushRect(LayoutRect rect, ColorF color)
        {
            Contract.Requires<InvalidOperationException>(isBuilt == false);

            DisplayListBuilder.wr_dp_push_rect(
                this.NativePtr,
                rect,
                0, // TODO: Expose IsBackfaceVisible
                color
            );
        }

        public uint DefineClip(LayoutRect clip)
        {
            return DisplayListBuilder.wr_dp_define_clip(this.NativePtr, IntPtr.Zero, clip, IntPtr.Zero, 0, IntPtr.Zero);
        }

        public void PushClip(uint clipId)
        {
            DisplayListBuilder.wr_dp_push_clip(this.NativePtr, clipId);
        }

        public void PopClip()
        {
            DisplayListBuilder.wr_dp_pop_clip(this.NativePtr);
        }

        public void PushText(
            WrFontInstanceKey fontKey,
            GlyphInstance[] glyphs,
            int glyphLen,
            LayoutRect rect,
            ColorF color
        )
        {
            var go = new GlyphOptions { RenderMode = FontRenderMode.Subpixel };
            DisplayListBuilder.wr_dp_push_text(
                this.NativePtr,
                rect,
                0,
                color,
                fontKey,
                glyphs,
                glyphLen,
                ref go
            );
        }

        public void PushReferenceFrame(
            LayoutRect bounds,
            LayoutTransform transform,
            ref WrAnimationProperty opacity,
            float opacityVal
        )
        {
            float opacityValue = opacityVal;
            DisplayListBuilder.wr_dp_push_stacking_context(
                this.NativePtr,
                bounds,
                IntPtr.Zero,
                ref opacity,
                ref opacityVal,
                ref transform,
                TransformStyle.Flat,
                IntPtr.Zero,
                MixBlendMode.Normal,
                IntPtr.Zero,
                0,
                0,
                new GlyphRasterSpace(),
                out var is_reference_frame,
                out var reference_frame_id
            );
        }

        public void PopReferenceFrame()
        {
            DisplayListBuilder.wr_dp_pop_stacking_context(this.NativePtr);
        }

        public (BuiltDisplayListDescriptor, LayoutSize, WrVecU8) Build()
        {
            this.isBuilt = true;
            DisplayListBuilder.wr_api_finalize_builder(this.NativePtr, out var content_size, out var bdld, out var dl_data);
            return (bdld, content_size, dl_data);
        }

        public override void Dispose()
        {
            DisplayListBuilder.wr_state_delete(this.NativePtr);
        }
    }
}
