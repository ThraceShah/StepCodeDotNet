namespace StepCodeDotNet.Interop;

public unsafe partial struct Schema_
{
    [NativeTypeName("Linked_List")]
    public Linked_List_* rules;

    [NativeTypeName("Linked_List")]
    public Linked_List_* reflist;

    [NativeTypeName("Linked_List")]
    public Linked_List_* uselist;

    [NativeTypeName("Dictionary")]
    public Hash_Table_* refdict;

    [NativeTypeName("Dictionary")]
    public Hash_Table_* usedict;

    [NativeTypeName("Linked_List")]
    public Linked_List_* use_schemas;

    [NativeTypeName("Linked_List")]
    public Linked_List_* ref_schemas;
}
