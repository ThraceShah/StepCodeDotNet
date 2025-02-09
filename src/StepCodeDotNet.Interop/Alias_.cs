namespace StepCodeDotNet.Interop;

public unsafe partial struct Alias_
{
    [NativeTypeName("struct Scope_ *")]
    public Scope_* scope;

    [NativeTypeName("struct Variable_ *")]
    public Variable_* variable;

    [NativeTypeName("Linked_List")]
    public Linked_List_* statements;
}
