namespace StepCodeDotNet.Interop;

public unsafe partial struct Symbol_
{
    [NativeTypeName("char *")]
    public sbyte* name;

    [NativeTypeName("const char *")]
    public sbyte* filename;

    public int line;

    [NativeTypeName("char")]
    public sbyte resolved;
}
