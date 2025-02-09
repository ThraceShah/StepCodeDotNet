namespace StepCodeDotNet.Interop;

public unsafe partial struct Conditional_
{
    [NativeTypeName("Expression")]
    public Expression_* test;

    [NativeTypeName("Linked_List")]
    public Linked_List_* code;

    [NativeTypeName("Linked_List")]
    public Linked_List_* otherwise;
}
