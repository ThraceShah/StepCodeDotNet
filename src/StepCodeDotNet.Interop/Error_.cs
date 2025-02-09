namespace StepCodeDotNet.Interop;

public unsafe partial struct Error_
{
    [NativeTypeName("enum Severity")]
    public Severity severity;

    [NativeTypeName("const char *")]
    public sbyte* message;

    [NativeTypeName("const char *")]
    public sbyte* name;

    [NativeTypeName("_Bool")]
    public bool @override;
}
