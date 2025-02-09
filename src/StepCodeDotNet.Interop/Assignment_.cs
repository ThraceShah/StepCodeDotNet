namespace StepCodeDotNet.Interop;

public unsafe partial struct Assignment_
{
    [NativeTypeName("Expression")]
    public Expression_* lhs;

    [NativeTypeName("Expression")]
    public Expression_* rhs;
}
