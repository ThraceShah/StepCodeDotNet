namespace StepCodeDotNet.Interop;

public unsafe partial struct Case_Statement_
{
    [NativeTypeName("Expression")]
    public Expression_* selector;

    [NativeTypeName("Linked_List")]
    public Linked_List_* cases;
}
