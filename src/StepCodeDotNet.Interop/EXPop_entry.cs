namespace StepCodeDotNet.Interop;

public unsafe partial struct EXPop_entry
{
    [NativeTypeName("char *")]
    public sbyte* token;

    [NativeTypeName("Type (*)(Expression, struct Scope_ *)")]
    public delegate* unmanaged[Cdecl]<Expression_*, Scope_*, Scope_*> resolve;
}
