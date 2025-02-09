namespace StepCodeDotNet.Interop;

public unsafe partial struct Op_Subexpression
{
    public Op_Code op_code;

    [NativeTypeName("Expression")]
    public Expression_* op1;

    [NativeTypeName("Expression")]
    public Expression_* op2;

    [NativeTypeName("Expression")]
    public Expression_* op3;
}
