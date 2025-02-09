namespace StepCodeDotNet.Interop;

public unsafe partial struct FullText
{
    [NativeTypeName("const char *")]
    public sbyte* filename;

    [NativeTypeName("unsigned int")]
    public uint start;

    [NativeTypeName("unsigned int")]
    public uint end;
}
