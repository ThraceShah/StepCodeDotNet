namespace StepCodeDotNet.Interop;

public unsafe partial struct Loop_
{
    [NativeTypeName("struct Scope_ *")]
    public Scope_* scope;

    [NativeTypeName("Expression")]
    public Expression_* while_expr;

    [NativeTypeName("Expression")]
    public Expression_* until_expr;

    [NativeTypeName("Linked_List")]
    public Linked_List_* statements;
}
