namespace StepCodeDotNet.Interop;

public unsafe partial struct Express_
{
    [NativeTypeName("FILE *")]
    public void* file;

    [NativeTypeName("char *")]
    public sbyte* filename;

    [NativeTypeName("char *")]
    public sbyte* basename;
}
