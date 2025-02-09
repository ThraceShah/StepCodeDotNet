namespace StepCodeDotNet.Interop;

public unsafe partial struct Object
{
    [NativeTypeName("struct Symbol_ *(*)(void *)")]
    public delegate* unmanaged[Cdecl]<void*, Symbol_*> get_symbol;

    [NativeTypeName("char *")]
    public sbyte* type;

    public int bits;
}
