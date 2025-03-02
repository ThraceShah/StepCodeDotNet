using System.Runtime.CompilerServices;
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

public unsafe static class ScopeEx
{
    /*
    #define ENTITYget_attributes(e) ((e)->u.entity->attributes)
    #define ENTITYget_subtypes(e)   ((e)->u.entity->subtypes)
    #define ENTITYget_supertypes(e) ((e)->u.entity->supertypes)
    #define ENTITYget_name(e)   ((e)->symbol.name)
    #define ENTITYget_uniqueness_list(e) ((e)->u.entity->unique)
    #define ENTITYget_where(e)  ((e)->where)
    #define ENTITYput_where(e,w)    ((e)->where = (w))
    #define ENTITYget_abstract(e)   ((e)->u.entity->abstract)
    #define ENTITYget_mark(e)   ((e)->u.entity->mark)
    #define ENTITYput_mark(e,m) ((e)->u.entity->mark = (m))
    #define ENTITYput_inheritance_count(e,c) ((e)->u.entity->inheritance = (c))
    #define ENTITYget_inheritance_count(e)  ((e)->u.entity->inheritance)
    #define ENTITYget_size(e)   ((e)->u.entity->attribute_count + (e)->u.entity->inheritance)
    #define ENTITYget_attribute_count(e)    ((e)->u.entity->attribute_count)
    #define ENTITYput_attribute_count(e,c)  ((e)->u.entity->attribute_count = (c))

    #define ENTITYget_clientData(e)     ((e)->clientData)
    #define ENTITYput_clientData(e,d)       ((e)->clientData = (d))

    */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Linked_List_* ENTITYget_attributes(Scope_* scope)
    {
        return scope->u.entity->attributes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Linked_List_* ENTITYget_subtypes(Scope_* scope)
    {
        return scope->u.entity->subtypes;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Linked_List_* ENTITYget_supertypes(Scope_* scope)
    {
        return scope->u.entity->supertypes;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte* ENTITYget_name(Scope_* scope)
    {
        return scope->symbol.name;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Linked_List_* ENTITYget_uniqueness_list(Scope_* scope)
    {
        return scope->u.entity->unique;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Linked_List_* ENTITYget_where(Scope_* scope)
    {
        return scope->where;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ENTITYput_where(Scope_* scope, Linked_List_* where)
    {
        scope->where = where;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ENTITYget_abstract(Scope_* scope)
    {
        return scope->u.entity->@abstract;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ENTITYget_mark(Scope_* scope)
    {
        return scope->u.entity->mark;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ENTITYput_mark(Scope_* scope, int mark)
    {
        scope->u.entity->mark = mark;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ENTITYput_inheritance_count(Scope_* scope, int count)
    {
        scope->u.entity->inheritance = count;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ENTITYget_inheritance_count(Scope_* scope)
    {
        return scope->u.entity->inheritance;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ENTITYget_size(Scope_* scope)
    {
        return scope->u.entity->attribute_count + scope->u.entity->inheritance;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ENTITYget_attribute_count(Scope_* scope)
    {
        return scope->u.entity->attribute_count;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ENTITYput_attribute_count(Scope_* scope, int count)
    {
        scope->u.entity->attribute_count = count;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ENTITYput_clientData(Scope_* scope, void* data)
    {
        scope->clientData = data;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ENTITYget_clientData(Scope_* scope)
    {
        return scope->clientData;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Scope_* TYPEget_base_type(Scope_* t)
    {
        return t->u.type->body->@base;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TYPEis_enumeration(Scope_* t)
    {
        return t->u.type->body->type == type_enum.enumeration_;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TYPEis_select(Scope_* t)
    {
        return t->u.type->body->type == type_enum.select_;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TYPEis_aggregate(Scope_* t)
    {
        return t->u.type->body->@base != null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TYPEis_aggregate_raw(Scope_* t)
    {
        return t->u.type->body->type == type_enum.aggregate_;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Scope_* TYPEget_head(Scope_* t)
    {
        return t->u.type->head;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeBody_* TYPEget_body(Scope_* t)
    {
        return t->u.type->body;
    }
}