using Avalonia.WebRender.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Text
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WrFontInfo
    {
        public WrFontKey FontKey;
        public IntPtr FontDesc;
    }

    internal static class FontCache
    {
        [DllImport("webrender_bindings.dll")]
        public static extern WrFontInfo wr_font_key_from_properties(
            IntPtr document,
            string font_name,
            int fontWeight,
            int fontStyle,
            int fontStretch
        );

        static readonly Dictionary<IntPtr, Dictionary<(int, WrFontKey), WrFontInstanceKey>> Cache = new Dictionary<IntPtr, Dictionary<(int, WrFontKey), WrFontInstanceKey>>();
        static readonly Dictionary<(IntPtr, string, int, int, int), WrFontKey> FontKeyCache = new Dictionary<(IntPtr, string, int, int, int), WrFontKey>();

        public static WrFontKey GetFontKey(Document document, string font, int fontWeight, int fontStyle, int fontStretch)
        {
            if (!FontKeyCache.ContainsKey((document.NativePtr, font, fontWeight, fontStyle, fontStretch)))
            {
                System.Diagnostics.Debug.WriteLine("Adding a WrFontKey to the FontKeyCache for {0} {1} {2} {3} {4}", font, fontWeight, fontStyle, fontStretch, document.NativePtr);
                var info = FontCache.wr_font_key_from_properties(
                    document.NativePtr,
                    font,
                    fontWeight,
                    fontStyle,
                    fontStretch
                );

                using (var txn = new Transaction(document))
                {
                    txn.AddFontDescriptor(info.FontDesc, info.FontKey);
                }

                FontKeyCache[(document.NativePtr, font, fontWeight, fontStyle, fontStretch)] = info.FontKey;
            }

            return FontKeyCache[(document.NativePtr, font, fontWeight, fontStyle, fontStretch)];
        }

        public static WrFontInstanceKey GetFontInstance(Document document, WrFontKey key, int fontSize)
        {
            Dictionary<(int, WrFontKey), WrFontInstanceKey> entry;

            if (!Cache.TryGetValue(document.NativePtr, out entry))
            {
                Cache[document.NativePtr] = entry = new Dictionary<(int, WrFontKey), WrFontInstanceKey>();
            }

            WrFontInstanceKey fontInstance = new WrFontInstanceKey();

            if (!entry.TryGetValue((fontSize, key), out fontInstance))
            {
                using (var txn = new Transaction(document))
                {
                    fontInstance = txn.AddFontInstance(key, fontSize);
                    System.Diagnostics.Debug.WriteLine("Adding a WrFontInstanceKey to the FontKeyCache for {0}", fontSize);
                }

                entry[(fontSize, key)] = fontInstance;
            }

            return fontInstance;
        }
    }
}
