namespace StepCodeDotNet.Interop;

public unsafe partial struct HashEntry
{
    [NativeTypeName("unsigned int")]
    public uint i;

    [NativeTypeName("unsigned int")]
    public uint j;

    [NativeTypeName("Element")]
    public Element_* p;

    [NativeTypeName("Hash_Table")]
    public Hash_Table_* table;

    [NativeTypeName("char")]
    public sbyte type;

    [NativeTypeName("Element")]
    public Element_* e;
}
