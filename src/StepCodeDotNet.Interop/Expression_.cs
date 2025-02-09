namespace StepCodeDotNet.Interop;

public unsafe partial struct Expression_
{
    [NativeTypeName("Symbol")]
    public Symbol_ symbol;

    [NativeTypeName("Type")]
    public Scope_* type;

    [NativeTypeName("Type")]
    public Scope_* return_type;

    [NativeTypeName("struct Op_Subexpression")]
    public Op_Subexpression e;

    [NativeTypeName("union expr_union")]
    public expr_union u;
}
