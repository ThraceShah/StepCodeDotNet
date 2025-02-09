namespace StepCodeDotNet.Interop;

public unsafe partial struct Funcall
{
    [NativeTypeName("struct Scope_ *")]
    public Scope_* function;

    [NativeTypeName("Linked_List")]
    public Linked_List_* list;
}
