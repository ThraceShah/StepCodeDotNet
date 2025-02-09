namespace StepCodeDotNet.Interop;

public unsafe partial struct tag
{
    [NativeTypeName("char *")]
    public sbyte* name;

    [NativeTypeName("Type")]
    public Scope_* type;
}
