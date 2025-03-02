using System.Runtime.CompilerServices;

namespace StepCodeDotNet.Interop;

public unsafe partial struct Variable_
{
    [NativeTypeName("Expression")]
    public Expression_* name;

    [NativeTypeName("Type")]
    public Scope_* type;

    [NativeTypeName("Expression")]
    public Expression_* initializer;

    public int offset;

    public int idx;

    [NativeTypeName("__AnonymousRecord_variable_L85_C5")]
    public _flags_e__Struct flags;

    [NativeTypeName("Symbol *")]
    public Symbol_* inverse_symbol;

    [NativeTypeName("Variable")]
    public Variable_* inverse_attribute;

    public partial struct _flags_e__Struct
    {
        public uint _bitfield;

        [NativeTypeName("unsigned int : 1")]
        public uint optional
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return _bitfield & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
            }
        }

        [NativeTypeName("unsigned int : 1")]
        public uint var
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 1) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1);
            }
        }

        [NativeTypeName("unsigned int : 1")]
        public uint constant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 2) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2);
            }
        }

        [NativeTypeName("unsigned int : 1")]
        public uint unique
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 3) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3);
            }
        }

        [NativeTypeName("unsigned int : 1")]
        public uint parameter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 4) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 4)) | ((value & 0x1u) << 4);
            }
        }

        [NativeTypeName("unsigned int : 1")]
        public uint attribute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 5) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 5)) | ((value & 0x1u) << 5);
            }
        }
    }
}

public static unsafe class VariableEx
{
    /*
    #define VARget_name(v)          ((v)->name)
    #define VARput_name(v,n)        ((v)->name = (n))
    #define VARput_offset(v,off)        ((v)->offset = (off))
    #define VARget_offset(v)        ((v)->offset)

    #define VARget_initializer(v)       ((v)->initializer)
    #define VARget_type(v)          ((v)->type)
    #define VARget_optional(v)      ((v)->flags.optional)
    #define VARget_unique(v)        ((v)->flags.unique)

    #define VARis_derived(v)        ((v)->initializer != 0)
    #define VARget_inverse(v)       ((v)->inverse_attribute)

    */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Expression_* VARget_name(Variable_* v) => v->name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VARput_name(Variable_* v, Expression_* n) => v->name = n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VARput_offset(Variable_* v, int off) => v->offset = off;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int VARget_offset(Variable_* v) => v->offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Expression_* VARget_initializer(Variable_* v) => v->initializer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Scope_* VARget_type(Variable_* v) => v->type;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint VARget_optional(Variable_* v) => v->flags.optional;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint VARget_unique(Variable_* v) => v->flags.unique;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool VARis_derived(Variable_* v) => v->initializer != null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variable_* VARget_inverse(Variable_* v) => v->inverse_attribute;
}