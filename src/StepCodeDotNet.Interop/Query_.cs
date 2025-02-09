namespace StepCodeDotNet.Interop;

public unsafe partial struct Query_
{
    [NativeTypeName("Variable")]
    public Variable_* local;

    [NativeTypeName("Expression")]
    public Expression_* aggregate;

    [NativeTypeName("Expression")]
    public Expression_* expression;

    [NativeTypeName("struct Scope_ *")]
    public Scope_* scope;
}
