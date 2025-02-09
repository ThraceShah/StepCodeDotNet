namespace StepCodeDotNet.Interop;

public unsafe partial struct Increment_
{
    [NativeTypeName("Expression")]
    public Expression_* init;

    [NativeTypeName("Expression")]
    public Expression_* end;

    [NativeTypeName("Expression")]
    public Expression_* increment;
}
