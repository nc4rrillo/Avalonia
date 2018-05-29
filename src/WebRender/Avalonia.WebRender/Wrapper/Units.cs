using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.WebRender.Wrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LayoutSize
    {
        public float width;
        public float height;

        public LayoutSize(float width, float height)
        {
            this.height = height;
            this.width = width;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LayoutPoint
    {
        public float x;
        public float y;

        public LayoutPoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LayoutRect
    {
        public LayoutPoint origin;
        public LayoutSize size;

        public LayoutRect(LayoutPoint origin, LayoutSize size)
        {
            this.origin = origin;
            this.size = size;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LayoutTransform
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;

        public float M21;
        public float M22;
        public float M23;
        public float M24;

        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public LayoutTransform(
            float M11, float M12, float M13, float M14,
            float M21, float M22, float M23, float M24,
            float M31, float M32, float M33, float M34,
            float M41, float M42, float M43, float M44
        )
        {
            this.M11 = M11;
            this.M12 = M12;
            this.M13 = M13;
            this.M14 = M14;

            this.M21 = M21;
            this.M22 = M22;
            this.M23 = M23;
            this.M24 = M24;

            this.M31 = M31;
            this.M32 = M32;
            this.M33 = M33;
            this.M34 = M34;

            this.M41 = M41;
            this.M42 = M42;
            this.M43 = M43;
            this.M44 = M44;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceUintPoint
    {
        public UInt32 x;
        public UInt32 y;

        public DeviceUintPoint(UInt32 x, UInt32 y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceUintSize
    {
        public UInt32 width;
        public UInt32 height;

        public DeviceUintSize(UInt32 width, UInt32 height)
        {
            this.height = height;
            this.width = width;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceUintRect
    {
        public DeviceUintPoint origin;
        public DeviceUintSize size;

        public DeviceUintRect(DeviceUintPoint origin, DeviceUintSize size)
        {
            this.origin = origin;
            this.size = size;
        }
    }
}
