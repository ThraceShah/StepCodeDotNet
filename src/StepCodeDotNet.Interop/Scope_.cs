using System.Runtime.InteropServices;

namespace StepCodeDotNet.Interop;

public unsafe partial struct Scope_
{
    [NativeTypeName("Symbol")]
    public Symbol_ symbol;

    [NativeTypeName("char")]
    public sbyte type;

    [NativeTypeName("ClientData")]
    public void* clientData;

    public int search_id;

    [NativeTypeName("Dictionary")]
    public Hash_Table_* symbol_table;

    [NativeTypeName("Dictionary")]
    public Hash_Table_* enum_table;

    [NativeTypeName("struct Scope_ *")]
    public Scope_* superscope;

    [NativeTypeName("__AnonymousRecord_scope_L87_C5")]
    public _u_e__Union u;

    [NativeTypeName("Linked_List")]
    public Linked_List_* where;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct _u_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("struct Procedure_ *")]
        public Procedure_* proc;

        [FieldOffset(0)]
        [NativeTypeName("struct Function_ *")]
        public Function_* func;

        [FieldOffset(0)]
        [NativeTypeName("struct Rule_ *")]
        public Rule_* rule;

        [FieldOffset(0)]
        [NativeTypeName("struct Entity_ *")]
        public Entity_* entity;

        [FieldOffset(0)]
        [NativeTypeName("struct Schema_ *")]
        public Schema_* schema;

        [FieldOffset(0)]
        [NativeTypeName("struct Express_ *")]
        public Express_* express;

        [FieldOffset(0)]
        [NativeTypeName("struct Increment_ *")]
        public Increment_* incr;

        [FieldOffset(0)]
        [NativeTypeName("struct TypeHead_ *")]
        public TypeHead_* type;
    }
}
