using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PipelineId
    {
        uint p0;
        uint p1;

        public PipelineId(uint p0, uint p1)
        {
            this.p0 = p0;
            this.p1 = p1;
        }
    }
}
