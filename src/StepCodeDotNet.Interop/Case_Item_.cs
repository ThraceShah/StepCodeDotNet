namespace StepCodeDotNet.Interop;

public unsafe partial struct Case_Item_
{
    [NativeTypeName("Symbol")]
    public Symbol_ symbol;

    [NativeTypeName("Linked_List")]
    public Linked_List_* labels;

    [NativeTypeName("struct Statement_ *")]
    public Statement_* action;
}
