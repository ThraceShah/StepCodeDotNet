namespace StepCodeDotNet.Interop;

public unsafe partial struct Where_
{
    [NativeTypeName("Symbol *")]
    public Symbol_* label;

    [NativeTypeName("Expression")]
    public Expression_* expr;
}
