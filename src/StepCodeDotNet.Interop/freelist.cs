using System.Runtime.InteropServices;

namespace StepCodeDotNet.Interop;

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct freelist
{
    [FieldOffset(0)]
    [NativeTypeName("union freelist *")]
    public freelist* next;

    [FieldOffset(0)]
    [NativeTypeName("char")]
    public sbyte memory;

    [FieldOffset(0)]
    [NativeTypeName("Align")]
    public nint aligner;
}
