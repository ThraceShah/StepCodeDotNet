using System.Runtime.CompilerServices;

namespace StepCodeDotNet.Interop;

public unsafe partial struct TypeBody_
{
    [NativeTypeName("struct TypeHead_ *")]
    public TypeHead_* head;

    [NativeTypeName("enum type_enum")]
    public type_enum type;

    [NativeTypeName("__AnonymousRecord_type_L157_C5")]
    public _flags_e__Struct flags;

    [NativeTypeName("Type")]
    public Scope_* @base;

    [NativeTypeName("Type")]
    public Scope_* tag;

    [NativeTypeName("Expression")]
    public Expression_* precision;

    [NativeTypeName("Linked_List")]
    public Linked_List_* list;

    [NativeTypeName("Expression")]
    public Expression_* upper;

    [NativeTypeName("Expression")]
    public Expression_* lower;

    [NativeTypeName("struct Scope_ *")]
    public Scope_* entity;

    public partial struct _flags_e__Struct
    {
        public uint _bitfield;

        [NativeTypeName("unsigned int : 1")]
        public uint unique
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
        public uint optional
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
        public uint @fixed
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
        public uint shared
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
        public uint repeat
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
        public uint var
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

        [NativeTypeName("unsigned int : 1")]
        public uint encoded
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (_bitfield >> 6) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 6)) | ((value & 0x1u) << 6);
            }
        }
    }
}
