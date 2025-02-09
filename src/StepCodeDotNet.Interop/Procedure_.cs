namespace StepCodeDotNet.Interop;

public unsafe partial struct Procedure_
{
    public int pcount;

    public int tag_count;

    [NativeTypeName("Linked_List")]
    public Linked_List_* parameters;

    [NativeTypeName("Linked_List")]
    public Linked_List_* body;

    [NativeTypeName("struct FullText")]
    public FullText text;

    public int builtin;
}
