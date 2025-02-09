namespace StepCodeDotNet.Interop;

public unsafe partial struct Entity_
{
    [NativeTypeName("Linked_List")]
    public Linked_List_* supertype_symbols;

    [NativeTypeName("Linked_List")]
    public Linked_List_* supertypes;

    [NativeTypeName("Linked_List")]
    public Linked_List_* subtypes;

    [NativeTypeName("Expression")]
    public Expression_* subtype_expression;

    [NativeTypeName("Linked_List")]
    public Linked_List_* attributes;

    public int inheritance;

    public int attribute_count;

    [NativeTypeName("Linked_List")]
    public Linked_List_* unique;

    [NativeTypeName("Linked_List")]
    public Linked_List_* instances;

    public int mark;

    [NativeTypeName("_Bool")]
    public bool @abstract;

    [NativeTypeName("Type")]
    public Scope_* type;
}
