namespace StepCodeDotNet.Interop;

public unsafe partial struct Function_
{
    public int pcount;

    public int tag_count;

    [NativeTypeName("Linked_List")]
    public Linked_List_* parameters;

    [NativeTypeName("Linked_List")]
    public Linked_List_* body;

    [NativeTypeName("Type")]
    public Scope_* return_type;

    [NativeTypeName("struct FullText")]
    public FullText text;

    public int builtin;
}
