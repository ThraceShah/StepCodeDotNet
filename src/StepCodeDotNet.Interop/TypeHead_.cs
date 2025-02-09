namespace StepCodeDotNet.Interop;

public unsafe partial struct TypeHead_
{
    [NativeTypeName("Type")]
    public Scope_* head;

    [NativeTypeName("struct TypeBody_ *")]
    public TypeBody_* body;
}
