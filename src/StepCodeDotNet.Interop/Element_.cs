namespace StepCodeDotNet.Interop;

public unsafe partial struct Element_
{
    [NativeTypeName("char *")]
    public sbyte* key;

    [NativeTypeName("char *")]
    public sbyte* data;

    [NativeTypeName("struct Element_ *")]
    public Element_* next;

    [NativeTypeName("Symbol *")]
    public Symbol_* symbol;

    [NativeTypeName("char")]
    public sbyte type;
}
