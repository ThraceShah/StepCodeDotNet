namespace StepCodeDotNet.Interop;

public unsafe partial struct Rename
{
    [NativeTypeName("struct Symbol_ *")]
    public Symbol_* schema_sym;

    [NativeTypeName("Schema")]
    public Scope_* schema;

    [NativeTypeName("struct Symbol_ *")]
    public Symbol_* old;

    [NativeTypeName("struct Symbol_ *")]
    public Symbol_* nnew;

    public void* @object;

    [NativeTypeName("char")]
    public sbyte type;

    [NativeTypeName("enum rename_type")]
    public rename_type rename_type;

    public int userdata;
}
