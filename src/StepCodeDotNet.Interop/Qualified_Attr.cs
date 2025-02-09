namespace StepCodeDotNet.Interop;

public unsafe partial struct Qualified_Attr
{
    [NativeTypeName("struct Expression_ *")]
    public Expression_* _complex;

    [NativeTypeName("Symbol *")]
    public Symbol_* entity;

    [NativeTypeName("Symbol *")]
    public Symbol_* attribute;
}
