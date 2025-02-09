namespace StepCodeDotNet.Interop;

public unsafe partial struct Rule_
{
    [NativeTypeName("Linked_List")]
    public Linked_List_* parameters;

    [NativeTypeName("Linked_List")]
    public Linked_List_* body;

    [NativeTypeName("struct FullText")]
    public FullText text;
}
