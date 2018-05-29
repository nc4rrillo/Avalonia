using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.WebRender.Utils;
using Avalonia.WebRender.Wrapper;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalonia.WebRender.Text
{
    public class FormattedTextImpl : IFormattedTextImpl
    {
        public WrFontKey FontKey { get; private set; }
        public WrFontInstanceKey FontInstanceKey { get; private set; }

        public Size Constraint { get; private set; } = new Size(double.PositiveInfinity, double.PositiveInfinity);
        public Size Size { get; private set; }

        public int[] GlyphIndices { get; private set; } = null;
        public GlyphInstance[] GlyphInstances { get; private set; } = null;

        public string Text { get; }

        public void BuildFontKey(Document document, Point origin)
        {
            (var glyphs, var glyphCount) = this.CharToGlyphs();

            var positions = new List<LayoutPoint>();
            origin = origin.WithY(origin.Y - _paint.FontMetrics.Ascent);
            foreach (var metric in this.GetRects())
            {
                var m = metric.Translate(origin);
                positions.Add(new LayoutPoint(
                   (float)m.X,
                   (float)m.Y
                ));
            }

            var fontStyle = 0;
            if (this._paint.Typeface.Style == SKTypefaceStyle.Italic)
                fontStyle = 2;
            else if (this._paint.Typeface.Style == SKTypefaceStyle.BoldItalic)
                fontStyle = 2;

            this.FontKey = FontCache.GetFontKey(
                document,
                this._paint.Typeface.FamilyName,
                this._paint.Typeface.FontWeight,
                fontStyle,
                5
            );

            this.FontInstanceKey = FontCache.GetFontInstance(
                document,
                this.FontKey,
                (int)this._paint.TextSize
            );

            this.GlyphInstances = glyphs
                .Zip(positions, (glyphIndex, position) => (glyphIndex, position))
                .Select(fs => new GlyphInstance { Index = fs.glyphIndex, Point = fs.position }).ToArray();
        }

        public FormattedTextImpl(
                    string text,
                    Typeface typeface,
                    TextAlignment textAlignment,
                    TextWrapping wrapping,
                    Size constraint,
                    IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            Text = text ?? string.Empty;

            // Replace 0 characters with zero-width spaces (200B)
            Text = Text.Replace((char)0, (char)0x200B);


            var skiaTypeface = TypefaceCache.GetTypeface(
                typeface?.FontFamily.Name ?? "monospace",
                typeface?.Style ?? FontStyle.Normal,
                typeface?.Weight ?? FontWeight.Normal);

            _paint = new SKPaint();
            _paint.TextEncoding = SKTextEncoding.Utf16;
            _paint.IsStroke = false;
            _paint.IsAntialias = true;
            _paint.LcdRenderText = true;
            _paint.SubpixelText = true;
            _paint.Typeface = skiaTypeface;
            _paint.TextSize = (float)(typeface?.FontSize ?? 12);
            _paint.TextAlign = textAlignment.ToSKTextAlign();

            _wrapping = wrapping;
            Constraint = constraint;

            Rebuild();
        }

        public IEnumerable<FormattedTextLine> GetLines()
        {
            return _lines;
        }

        public TextHitTestResult HitTestPoint(Point point)
        {
            float y = (float)point.Y;
            var line = _skiaLines.Find(l => l.Top <= y && (l.Top + l.Height) > y);

            if (!line.Equals(default(AvaloniaFormattedTextLine)))
            {
                var rects = GetRects();

                for (int c = line.Start; c < line.Start + line.TextLength; c++)
                {
                    var rc = rects[c];
                    if (rc.Contains(point))
                    {
                        return new TextHitTestResult
                        {
                            IsInside = !(line.TextLength > line.Length),
                            TextPosition = c,
                            IsTrailing = (point.X - rc.X) > rc.Width / 2
                        };
                    }
                }

                int offset = 0;

                if (point.X >= (rects[line.Start].X + line.Width) / 2 && line.Length > 0)
                {
                    offset = line.TextLength > line.Length ?
                                    line.Length : (line.Length - 1);
                }

                return new TextHitTestResult
                {
                    IsInside = false,
                    TextPosition = line.Start + offset,
                    IsTrailing = Text.Length == (line.Start + offset + 1)
                };
            }

            bool end = point.X > Size.Width || point.Y > _lines.Sum(l => l.Height);

            return new TextHitTestResult()
            {
                IsInside = false,
                IsTrailing = end,
                TextPosition = end ? Text.Length - 1 : 0
            };
        }

        public Rect HitTestTextPosition(int index)
        {
            var rects = GetRects();

            if (index < 0 || index >= rects.Count)
            {
                var r = rects.LastOrDefault();
                return new Rect(r.X + r.Width, r.Y, 0, _lineHeight);
            }

            if (rects.Count == 0)
            {
                return new Rect(0, 0, 1, _lineHeight);
            }

            if (index == rects.Count)
            {
                var lr = rects[rects.Count - 1];
                return new Rect(new Point(lr.X + lr.Width, lr.Y), rects[index - 1].Size);
            }

            return rects[index];
        }

        public IEnumerable<Rect> HitTestTextRange(int index, int length)
        {
            List<Rect> result = new List<Rect>();

            var rects = GetRects();

            int lastIndex = index + length - 1;

            foreach (var line in _skiaLines.Where(l =>
                                                    (l.Start + l.Length) > index &&
                                                    lastIndex >= l.Start))
            {
                int lineEndIndex = line.Start + (line.Length > 0 ? line.Length - 1 : 0);

                double left = rects[line.Start > index ? line.Start : index].X;
                double right = rects[lineEndIndex > lastIndex ? lastIndex : lineEndIndex].Right;

                result.Add(new Rect(left, line.Top, right - left, line.Height));
            }

            return result;
        }

        public override string ToString()
        {
            return Text;
        }

        private const float MAX_LINE_WIDTH = 10000;
        private readonly List<FormattedTextLine> _lines = new List<FormattedTextLine>();
        private readonly TextWrapping _wrapping;
        private readonly SKPaint _paint;
        private readonly List<Rect> _rects = new List<Rect>();
        private List<AvaloniaFormattedTextLine> _skiaLines;
        private float _lineHeight = 0;
        private float _lineOffset = 0;

        public (int[], int) CharToGlyphs()
        {
            if (this.Text == "")
                return (new int[] { }, 0);

            this._paint.Typeface.CharsToGlyphs(this.Text, out var glyphs);
            return (glyphs.AsEnumerable().Select(g => (int)g).ToArray(), this._paint.Typeface.CountGlyphs(this.Text));
        }

        private static bool IsBreakChar(char c)
        {
            //white space or zero space whitespace
            return char.IsWhiteSpace(c) || c == '\u200B';
        }

        private static int LineBreak(string textInput, int textIndex, int stop,
                                     SKPaint paint, float maxWidth,
                                     out int trailingCount)
        {
            int lengthBreak;
            if (maxWidth == -1)
            {
                lengthBreak = stop - textIndex;
            }
            else
            {
                string subText = textInput.Substring(textIndex, stop - textIndex);
                lengthBreak = (int)paint.BreakText(subText, maxWidth, out float measuredWidth);
            }

            //Check for white space or line breakers before the lengthBreak
            int startIndex = textIndex;
            int index = textIndex;
            int word_start = textIndex;
            bool prevBreak = true;

            trailingCount = 0;

            while (index < stop)
            {
                int prevText = index;
                char currChar = textInput[index++];
                bool currBreak = IsBreakChar(currChar);

                if (!currBreak && prevBreak)
                {
                    word_start = prevText;
                }

                prevBreak = currBreak;

                if (index > startIndex + lengthBreak)
                {
                    if (currBreak)
                    {
                        // eat the rest of the whitespace
                        while (index < stop && IsBreakChar(textInput[index]))
                        {
                            index++;
                        }

                        trailingCount = index - prevText;
                    }
                    else
                    {
                        // backup until a whitespace (or 1 char)
                        if (word_start == startIndex)
                        {
                            if (prevText > startIndex)
                            {
                                index = prevText;
                            }
                        }
                        else
                        {
                            index = word_start;
                        }
                    }
                    break;
                }

                if ('\n' == currChar)
                {
                    int ret = index - startIndex;
                    int lineBreakSize = 1;
                    if (index < stop)
                    {
                        currChar = textInput[index++];
                        if ('\r' == currChar)
                        {
                            ret = index - startIndex;
                            ++lineBreakSize;
                        }
                    }

                    trailingCount = lineBreakSize;

                    return ret;
                }

                if ('\r' == currChar)
                {
                    int ret = index - startIndex;
                    int lineBreakSize = 1;
                    if (index < stop)
                    {
                        currChar = textInput[index++];
                        if ('\n' == currChar)
                        {
                            ret = index - startIndex;
                            ++lineBreakSize;
                        }
                    }

                    trailingCount = lineBreakSize;

                    return ret;
                }
            }

            return index - startIndex;
        }

        private void BuildRects()
        {
            // Build character rects
            SKTextAlign align = _paint.TextAlign;

            for (int li = 0; li < _skiaLines.Count; li++)
            {
                var line = _skiaLines[li];
                float prevRight = TransformX(0, line.Width, align);
                double nextTop = line.Top + line.Height;

                if (li + 1 < _skiaLines.Count)
                {
                    nextTop = _skiaLines[li + 1].Top;
                }

                for (int i = line.Start; i < line.Start + line.TextLength; i++)
                {
                    float w = _paint.MeasureText(Text[i].ToString());

                    _rects.Add(new Rect(
                        prevRight,
                        line.Top,
                        w,
                        nextTop - line.Top));
                    prevRight += w;
                }
            }
        }


        private List<Rect> GetRects()
        {
            if (Text.Length > _rects.Count)
            {
                BuildRects();
            }

            return _rects;
        }

        private void Rebuild()
        {
            var length = Text.Length;

            _lines.Clear();
            _rects.Clear();
            _skiaLines = new List<AvaloniaFormattedTextLine>();

            int curOff = 0;
            float curY = 0;

            var metrics = _paint.FontMetrics;

            var mTop = metrics.Top;  // The greatest distance above the baseline for any glyph (will be <= 0).
            var mBottom = metrics.Bottom;  // The greatest distance below the baseline for any glyph (will be >= 0).
            var mLeading = metrics.Leading;  // The recommended distance to add between lines of text (will be >= 0).
            var mDescent = metrics.Descent;  //The recommended distance below the baseline. Will be >= 0.
            var mAscent = metrics.Ascent;    //The recommended distance above the baseline. Will be <= 0.
            var lastLineDescent = mBottom - mDescent;

            // This seems like the best measure of full vertical extent
            // matches Direct2D line height
            _lineHeight = mDescent - mAscent;

            // Rendering is relative to baseline
            _lineOffset = (-metrics.Ascent);

            string subString;

            float widthConstraint = double.IsPositiveInfinity(Constraint.Width)
                                        ? -1
                                        : (float)Constraint.Width;

            while (curOff < length)
            {
                float lineWidth = -1;
                int measured;
                int trailingnumber = 0;

                float constraint = -1;

                if (_wrapping == TextWrapping.Wrap)
                {
                    constraint = widthConstraint <= 0 ? MAX_LINE_WIDTH : widthConstraint;
                    if (constraint > MAX_LINE_WIDTH)
                        constraint = MAX_LINE_WIDTH;
                }

                measured = LineBreak(Text, curOff, length, _paint, constraint, out trailingnumber);

                AvaloniaFormattedTextLine line = new AvaloniaFormattedTextLine();
                line.TextLength = measured;

                subString = Text.Substring(line.Start, line.TextLength);
                lineWidth = _paint.MeasureText(subString);
                line.Start = curOff;
                line.Length = measured - trailingnumber;
                line.Width = lineWidth;
                line.Height = _lineHeight;
                line.Top = curY;

                _skiaLines.Add(line);

                curY += _lineHeight;

                curY += mLeading;

                curOff += measured;
            }

            // Now convert to Avalonia data formats
            _lines.Clear();
            float maxX = 0;

            for (var c = 0; c < _skiaLines.Count; c++)
            {
                var w = _skiaLines[c].Width;
                if (maxX < w)
                    maxX = w;

                _lines.Add(new FormattedTextLine(_skiaLines[c].TextLength, _skiaLines[c].Height));
            }

            if (_skiaLines.Count == 0)
            {
                _lines.Add(new FormattedTextLine(0, _lineHeight));
                Size = new Size(0, _lineHeight);
            }
            else
            {
                var lastLine = _skiaLines[_skiaLines.Count - 1];
                Size = new Size(maxX, lastLine.Top + lastLine.Height);
            }
        }

        private float TransformX(float originX, float lineWidth, SKTextAlign align)
        {
            float x = 0;

            if (align == SKTextAlign.Left)
            {
                x = originX;
            }
            else
            {
                double width = Constraint.Width > 0 && !double.IsPositiveInfinity(Constraint.Width) ?
                                Constraint.Width :
                                Size.Width;

                switch (align)
                {
                    case SKTextAlign.Center: x = originX + (float)(width - lineWidth) / 2; break;
                    case SKTextAlign.Right: x = originX + (float)(width - lineWidth); break;
                }
            }

            return x;
        }

        private struct AvaloniaFormattedTextLine
        {
            public float Height;
            public int Length;
            public int Start;
            public int TextLength;
            public float Top;
            public float Width;
        };
    }
}
