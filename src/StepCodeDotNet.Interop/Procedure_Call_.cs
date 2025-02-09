namespace StepCodeDotNet.Interop;

public unsafe partial struct Procedure_Call_
{
    [NativeTypeName("struct Scope_ *")]
    public Scope_* procedure;

    [NativeTypeName("Linked_List")]
    public Linked_List_* parameters;
}
