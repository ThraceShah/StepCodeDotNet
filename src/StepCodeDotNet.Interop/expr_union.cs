using System.Runtime.InteropServices;

namespace StepCodeDotNet.Interop;

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct expr_union
{
    [FieldOffset(0)]
    public int integer;

    [FieldOffset(0)]
    public double real;

    [FieldOffset(0)]
    [NativeTypeName("char *")]
    public sbyte* attribute;

    [FieldOffset(0)]
    [NativeTypeName("char *")]
    public sbyte* binary;

    [FieldOffset(0)]
    public int logical;

    [FieldOffset(0)]
    [NativeTypeName("_Bool")]
    public bool boolean;

    [FieldOffset(0)]
    [NativeTypeName("struct Query_ *")]
    public Query_* query;

    [FieldOffset(0)]
    [NativeTypeName("struct Funcall")]
    public Funcall funcall;

    [FieldOffset(0)]
    [NativeTypeName("Linked_List")]
    public Linked_List_* list;

    [FieldOffset(0)]
    [NativeTypeName("Expression")]
    public Expression_* expression;

    [FieldOffset(0)]
    [NativeTypeName("struct Scope_ *")]
    public Scope_* entity;

    [FieldOffset(0)]
    [NativeTypeName("Variable")]
    public Variable_* variable;
}
