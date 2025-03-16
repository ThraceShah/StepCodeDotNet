using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace StepCodeDotNet.Interop;
#pragma warning disable CS8603 // Possible null reference return.

public static unsafe partial class IExpress
{
    static readonly nint ExpressHandle;
    static readonly sbyte* __ERROR_buffer_errors;
    static readonly Scan_Buffer* __SCAN_buffers;
    static readonly int* __SCAN_current_buffer;
    static readonly sbyte* __SCANcurrent;
    static readonly sbyte* __EXPRESSprogram_name;
    static readonly nint* __ERRORusage_function;
    static readonly nint* __EXPRESSinit_args;
    static readonly nint* __EXPRESSbackend;
    static readonly nint* __EXPRESSsucceed;
    static readonly nint* __EXPRESSgetopt;
    static readonly nint* __ERRORoccurred;

    static bool ErrorBufferErrors
    {
        get => *__ERROR_buffer_errors != 0;
        set => *__ERROR_buffer_errors = value ? (sbyte)1 : (sbyte)0;
    }

    public static string EXPRESSprogram_name
    {
        get => Marshal.PtrToStringAnsi((nint)__EXPRESSprogram_name);
        set
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Marshal.Copy(bytes, 0, (nint)__EXPRESSprogram_name, bytes.Length);
        }
    }

    public static delegate* unmanaged[Cdecl]<void> ERRORusage_function
    {
        get => (delegate* unmanaged[Cdecl]<void>)(*__ERRORusage_function);
        set => *__ERRORusage_function = (nint)value;
    }

    public static delegate* unmanaged[Cdecl]<int, byte**, void> EXPRESSinit_args
    {
        get => (delegate* unmanaged[Cdecl]<int, byte**, void>)(*__EXPRESSinit_args);
        set => *__EXPRESSinit_args = (nint)value;
    }

    public static delegate* unmanaged[Cdecl]<Scope_*, void> EXPRESSbackend
    {
        get => (delegate* unmanaged[Cdecl]<Scope_*, void>)(*__EXPRESSbackend);
        set => *__EXPRESSbackend = (nint)value;
    }

    public static delegate* unmanaged[Cdecl]<Scope_*, int> EXPRESSsucceed
    {
        get => (delegate* unmanaged[Cdecl]<Scope_*, int>)(*__EXPRESSsucceed);
        set => *__EXPRESSsucceed = (nint)value;
    }

    public static delegate* unmanaged[Cdecl]<int, sbyte*, int> EXPRESSgetopt
    {
        get => (delegate* unmanaged[Cdecl]<int, sbyte*, int>)(*__EXPRESSgetopt);
        set => *__EXPRESSgetopt = (nint)value;
    }

    public static bool ERRORoccurred => *__ERRORoccurred != 0;

    static IExpress()
    {
        ExpressHandle = NativeLibrary.Load("express");
        __ERROR_buffer_errors = (sbyte*)NativeLibrary.GetExport(ExpressHandle, "__ERROR_buffer_errors");
        __SCAN_buffers = (Scan_Buffer*)NativeLibrary.GetExport(ExpressHandle, "SCAN_buffers");
        __SCAN_current_buffer = (int*)NativeLibrary.GetExport(ExpressHandle, "SCAN_current_buffer");
        __SCANcurrent = (sbyte*)NativeLibrary.GetExport(ExpressHandle, "SCANcurrent");
        __EXPRESSprogram_name = (sbyte*)NativeLibrary.GetExport(ExpressHandle, "EXPRESSprogram_name");
        __ERRORusage_function = (nint*)NativeLibrary.GetExport(ExpressHandle, "ERRORusage_function");
        __EXPRESSinit_args = (nint*)NativeLibrary.GetExport(ExpressHandle, "EXPRESSinit_args");
        __EXPRESSbackend = (nint*)NativeLibrary.GetExport(ExpressHandle, "EXPRESSbackend");
        __EXPRESSsucceed = (nint*)NativeLibrary.GetExport(ExpressHandle, "EXPRESSsucceed");
        __EXPRESSgetopt = (nint*)NativeLibrary.GetExport(ExpressHandle, "EXPRESSgetopt");
        __ERRORoccurred = (nint*)NativeLibrary.GetExport(ExpressHandle, "ERRORoccurred");
    }

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void DICTinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void DICTcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int DICTdefine([NativeTypeName("Dictionary")] Hash_Table_* param0, [NativeTypeName("char *")] sbyte* param1, void* param2, [NativeTypeName("Symbol *")] Symbol_* param3, [NativeTypeName("char")] sbyte param4);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int DICT_define([NativeTypeName("Dictionary")] Hash_Table_* param0, [NativeTypeName("char *")] sbyte* param1, void* param2, [NativeTypeName("Symbol *")] Symbol_* param3, [NativeTypeName("char")] sbyte param4);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void DICTundefine([NativeTypeName("Dictionary")] Hash_Table_* param0, [NativeTypeName("char *")] sbyte* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* DICTlookup([NativeTypeName("Dictionary")] Hash_Table_* param0, [NativeTypeName("char *")] sbyte* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* DICTlookup_symbol([NativeTypeName("Dictionary")] Hash_Table_* param0, [NativeTypeName("char *")] sbyte* param1, [NativeTypeName("Symbol **")] Symbol_** param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* DICTdo([NativeTypeName("DictionaryEntry *")] HashEntry* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void DICTprint([NativeTypeName("Dictionary")] Hash_Table_* param0);

    [NativeTypeName("#define DICTIONARY_NULL (Dictionary)NULL")]
    public static Hash_Table_* DICTIONARY_NULL => (Hash_Table_*)((void*)(0));

    [NativeTypeName("#define ENTITY_INHERITANCE_UNINITIALIZED -1")]
    public const int ENTITY_INHERITANCE_UNINITIALIZED = -1;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("struct Scope_ *")]
    public static extern Scope_* ENTITYcreate([NativeTypeName("struct Symbol_ *")] Symbol_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYadd_attribute([NativeTypeName("struct Scope_ *")] Scope_* param0, [NativeTypeName("struct Variable_ *")] Variable_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("struct Scope_ *")]
    public static extern Scope_* ENTITYcopy([NativeTypeName("struct Scope_ *")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Entity")]
    public static extern Scope_* ENTITYfind_inherited_entity([NativeTypeName("struct Scope_ *")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1, int param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Variable")]
    public static extern Variable_* ENTITYfind_inherited_attribute([NativeTypeName("struct Scope_ *")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1, [NativeTypeName("struct Symbol_ **")] Symbol_** param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Variable")]
    public static extern Variable_* ENTITYresolve_attr_ref([NativeTypeName("Entity")] Scope_* param0, [NativeTypeName("Symbol *")] Symbol_* param1, [NativeTypeName("Symbol *")] Symbol_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ENTITYhas_immediate_supertype([NativeTypeName("Entity")] Scope_* param0, [NativeTypeName("Entity")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Variable")]
    public static extern Variable_* ENTITYget_named_attribute([NativeTypeName("Entity")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* ENTITYget_all_attributes([NativeTypeName("Entity")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ENTITYhas_supertype([NativeTypeName("Entity")] Scope_* param0, [NativeTypeName("Entity")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYadd_instance([NativeTypeName("Entity")] Scope_* param0, void* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int ENTITYget_initial_offset([NativeTypeName("Entity")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int ENTITYdeclares_variable([NativeTypeName("Entity")] Scope_* param0, [NativeTypeName("struct Variable_ *")] Variable_* param1);

    [NativeTypeName("#define ENTITY_NULL (Entity)0")]
    public static Scope_* ENTITY_NULL => (Scope_*)(0);

    [NativeTypeName("#define ENTITY_get_symbol SCOPE_get_symbol")]
    public static delegate*<void*, Symbol_*> ENTITY_get_symbol => &SCOPE_get_symbol;

    [NativeTypeName("#define ERROR_MAX 100")]
    public const int ERROR_MAX = 100;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERROR_start_message_buffer();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERROR_flush_message_buffer();


    public static void ERRORbuffer_messages([NativeTypeName("_Bool")] bool flag)
    {
        ErrorBufferErrors = flag;
        if (flag)
        {
            ERROR_start_message_buffer();
        }
        else
        {
            ERROR_flush_message_buffer();
        }
    }

    public static void ERRORflush_messages()
    {
        if (ErrorBufferErrors)
        {
            ERROR_flush_message_buffer();
            ERROR_start_message_buffer();
        }
    }

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORnospace();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORabort(int param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORset_warning([NativeTypeName("char *")] sbyte* param0, [NativeTypeName("_Bool")] bool param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORset_all_warnings([NativeTypeName("_Bool")] bool param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORsafe([NativeTypeName("jmp_buf")] _SETJMP_FLOAT128* env);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ERRORunsafe();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* ERRORget_warnings_help([NativeTypeName("const char *")] sbyte* prefix, [NativeTypeName("const char *")] sbyte* eol);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ERRORis_enabled([NativeTypeName("enum ErrorCode")] ErrorCode errnum);

    [NativeTypeName("#define ERROR_none (Error)NULL")]
    public static Error_* ERROR_none => (Error_*)((void*)(0));

    [NativeTypeName("#define UNRESOLVED 0x0")]
    public const int UNRESOLVED = 0x0;

    [NativeTypeName("#define RESOLVED 0x1")]
    public const int RESOLVED = 0x1;

    [NativeTypeName("#define RESOLVE_FAILED 0x2")]
    public const int RESOLVE_FAILED = 0x2;

    [NativeTypeName("#define RESOLVE_IN_PROGRESS 0x4")]
    public const int RESOLVE_IN_PROGRESS = 0x4;

    [NativeTypeName("#define OBJ_ANY '*'")]
    public const int OBJ_ANY = (sbyte)('*');

    [NativeTypeName("#define OBJ_EXPRESS '!'")]
    public const int OBJ_EXPRESS = (sbyte)('!');

    [NativeTypeName("#define OBJ_PASS '#'")]
    public const int OBJ_PASS = (sbyte)('#');

    [NativeTypeName("#define OBJ_INCREMENT '+'")]
    public const int OBJ_INCREMENT = (sbyte)('+');

    [NativeTypeName("#define OBJ_ALIAS 'a'")]
    public const int OBJ_ALIAS = (sbyte)('a');

    [NativeTypeName("#define OBJ_QUERY 'q'")]
    public const int OBJ_QUERY = (sbyte)('q');

    [NativeTypeName("#define OBJ_PROCEDURE 'p'")]
    public const int OBJ_PROCEDURE = (sbyte)('p');

    [NativeTypeName("#define OBJ_RENAME 'n'")]
    public const int OBJ_RENAME = (sbyte)('n');

    [NativeTypeName("#define OBJ_RULE 'r'")]
    public const int OBJ_RULE = (sbyte)('r');

    [NativeTypeName("#define OBJ_FUNCTION 'f'")]
    public const int OBJ_FUNCTION = (sbyte)('f');

    [NativeTypeName("#define OBJ_TAG 'g'")]
    public const int OBJ_TAG = (sbyte)('g');

    [NativeTypeName("#define OBJ_ENTITY 'e'")]
    public const int OBJ_ENTITY = (sbyte)('e');

    [NativeTypeName("#define OBJ_SCHEMA 's'")]
    public const int OBJ_SCHEMA = (sbyte)('s');

    [NativeTypeName("#define OBJ_TYPE 't'")]
    public const int OBJ_TYPE = (sbyte)('t');

    [NativeTypeName("#define OBJ_UNKNOWN 'u'")]
    public const int OBJ_UNKNOWN = (sbyte)('u');

    [NativeTypeName("#define OBJ_VARIABLE 'v'")]
    public const int OBJ_VARIABLE = (sbyte)('v');

    [NativeTypeName("#define OBJ_WHERE 'w'")]
    public const int OBJ_WHERE = (sbyte)('w');

    [NativeTypeName("#define OBJ_EXPRESSION 'x'")]
    public const int OBJ_EXPRESSION = (sbyte)('x');

    [NativeTypeName("#define OBJ_ENUM 'x'")]
    public const int OBJ_ENUM = (sbyte)('x');

    [NativeTypeName("#define OBJ_AMBIG_ENUM 'z'")]
    public const int OBJ_AMBIG_ENUM = (sbyte)('z');

    [NativeTypeName("#define OBJ_TYPE_BITS 0x1")]
    public const int OBJ_TYPE_BITS = 0x1;

    [NativeTypeName("#define OBJ_ENTITY_BITS 0x2")]
    public const int OBJ_ENTITY_BITS = 0x2;

    [NativeTypeName("#define OBJ_FUNCTION_BITS 0x4")]
    public const int OBJ_FUNCTION_BITS = 0x4;

    [NativeTypeName("#define OBJ_PROCEDURE_BITS 0x8")]
    public const int OBJ_PROCEDURE_BITS = 0x8;

    [NativeTypeName("#define OBJ_PASS_BITS 0x10")]
    public const int OBJ_PASS_BITS = 0x10;

    [NativeTypeName("#define OBJ_RULE_BITS 0x20")]
    public const int OBJ_RULE_BITS = 0x20;

    [NativeTypeName("#define OBJ_EXPRESSION_BITS 0x40")]
    public const int OBJ_EXPRESSION_BITS = 0x40;

    [NativeTypeName("#define OBJ_SCHEMA_BITS 0x80")]
    public const int OBJ_SCHEMA_BITS = 0x80;

    [NativeTypeName("#define OBJ_VARIABLE_BITS 0x100")]
    public const int OBJ_VARIABLE_BITS = 0x100;

    [NativeTypeName("#define OBJ_WHERE_BITS 0x200")]
    public const int OBJ_WHERE_BITS = 0x200;

    [NativeTypeName("#define OBJ_ANYTHING_BITS 0x0fffffff")]
    public const int OBJ_ANYTHING_BITS = 0x0fffffff;

    [NativeTypeName("#define OBJ_UNFINDABLE_BITS 0x10000000")]
    public const int OBJ_UNFINDABLE_BITS = 0x10000000;

    [NativeTypeName("#define OBJ_ALGORITHM_BITS (OBJ_FUNCTION_BITS | OBJ_PROCEDURE_BITS | \\\r\n                 OBJ_RULE_BITS)")]
    public const int OBJ_ALGORITHM_BITS = (0x4 | 0x8 | 0x20);

    [NativeTypeName("#define OBJ_SCOPE_BITS (OBJ_ALGORITHM_BITS | OBJ_ENTITY_BITS | \\\r\n                 OBJ_SCHEMA_BITS)")]
    public const int OBJ_SCOPE_BITS = ((0x4 | 0x8 | 0x20) | 0x2 | 0x80);

    [NativeTypeName("#define OBJ_UNUSED_BITS OBJ_UNFINDABLE_BITS")]
    public const int OBJ_UNUSED_BITS = 0x10000000;

    [NativeTypeName("#define EXPRESSION_NULL (Expression)0")]
    public static Expression_* EXPRESSION_NULL => (Expression_*)(0);

    [NativeTypeName("#define MAXINT (~(1 << 31))")]
    public const int MAXINT = (~(1 << 31));

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* EXPcreate([NativeTypeName("Type")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* EXPcreate_simple([NativeTypeName("Type")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* EXPcreate_from_symbol([NativeTypeName("Type")] Scope_* param0, [NativeTypeName("Symbol *")] Symbol_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* UN_EXPcreate(Op_Code param0, [NativeTypeName("Expression")] Expression_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* BIN_EXPcreate(Op_Code param0, [NativeTypeName("Expression")] Expression_* param1, [NativeTypeName("Expression")] Expression_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* TERN_EXPcreate(Op_Code param0, [NativeTypeName("Expression")] Expression_* param1, [NativeTypeName("Expression")] Expression_* param2, [NativeTypeName("Expression")] Expression_* param3);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Expression")]
    public static extern Expression_* QUERYcreate([NativeTypeName("Symbol *")] Symbol_* param0, [NativeTypeName("Expression")] Expression_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* EXPtype([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("struct Scope_ *")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int EXPget_integer_value([NativeTypeName("Expression")] Expression_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* EXPresolve_op_dot([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Scope")] Scope_* param1);

    [NativeTypeName("#define EXPRESS_NULL (struct Scope_ *)0")]
    public static Scope_* EXPRESS_NULL => (Scope_*)(0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("IExpress")]
    public static extern Scope_* EXPRESScreate();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSdestroy([NativeTypeName("IExpress")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSparse([NativeTypeName("IExpress")] Scope_* param0, [NativeTypeName("FILE *")] _iobuf* param1, [NativeTypeName("char *")] sbyte* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESScleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSresolve([NativeTypeName("IExpress")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int EXPRESS_fail([NativeTypeName("IExpress")] Scope_* model);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int EXPRESS_succeed([NativeTypeName("IExpress")] Scope_* model);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void build_complex([NativeTypeName("IExpress")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void FACTORYinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void HASHinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Hash_Table")]
    public static extern Hash_Table_* HASHcreate([NativeTypeName("unsigned int")] uint param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Hash_Table")]
    public static extern Hash_Table_* HASHcopy([NativeTypeName("Hash_Table")] Hash_Table_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void HASHdestroy([NativeTypeName("Hash_Table")] Hash_Table_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Element")]
    public static extern Element_* HASHsearch([NativeTypeName("Hash_Table")] Hash_Table_* param0, [NativeTypeName("Element")] Element_* param1, Action param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void HASHlistinit([NativeTypeName("Hash_Table")] Hash_Table_* param0, HashEntry* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void HASHlistinit_by_type([NativeTypeName("Hash_Table")] Hash_Table_* param0, HashEntry* param1, [NativeTypeName("char")] sbyte param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Element")]
    public static extern Element_* HASHlist(HashEntry* param0);

    [NativeTypeName("#define HASH_NULL (Hash_Table)NULL")]
    public static Hash_Table_* HASH_NULL => (Hash_Table_*)((void*)(0));

    [NativeTypeName("#define SEGMENT_SIZE 256")]
    public const int SEGMENT_SIZE = 256;

    [NativeTypeName("#define SEGMENT_SIZE_SHIFT 8")]
    public const int SEGMENT_SIZE_SHIFT = 8;

    [NativeTypeName("#define DIRECTORY_SIZE 256")]
    public const int DIRECTORY_SIZE = 256;

    [NativeTypeName("#define DIRECTORY_SIZE_SHIFT 8")]
    public const int DIRECTORY_SIZE_SHIFT = 8;

    [NativeTypeName("#define PRIME1 37")]
    public const int PRIME1 = 37;

    [NativeTypeName("#define PRIME2 1048583")]
    public const int PRIME2 = 1048583;

    [NativeTypeName("#define MAX_LOAD_FACTOR 5")]
    public const int MAX_LOAD_FACTOR = 5;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* EXPRESSversion();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSusage(int _exit);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_real_literal([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_integer_literal([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_binary_literal([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_logical_literal([NativeTypeName("char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_identifier_or_keyword([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_string([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_encoded_string([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCANprocess_semicolon([NativeTypeName("const char *")] sbyte* param0, int param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANsave_comment([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SCANread();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANinclude_file([NativeTypeName("char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANlowerize([NativeTypeName("char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCANupperize([NativeTypeName("char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* SCANstrdup([NativeTypeName("const char *")] sbyte* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("long")]
    public static extern nint SCANtell();

    [NativeTypeName("#define SCAN_BUFFER_SIZE 1024")]
    public const int SCAN_BUFFER_SIZE = 1024;

    [NativeTypeName("#define SCAN_NESTING_DEPTH 6")]
    public const int SCAN_NESTING_DEPTH = 6;

    [NativeTypeName("#define SCAN_ESCAPE '\001'")]
    public const sbyte SCAN_ESCAPE = 1;

    [NativeTypeName("#define SCANbuffer SCAN_buffers[SCAN_current_buffer]")]
    public static ref readonly Scan_Buffer SCANbuffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(__SCAN_buffers + *__SCAN_current_buffer);
    }

    [NativeTypeName("#define SCANbol SCANbuffer.bol")]
    public static int SCANbol => __SCAN_buffers[*__SCAN_current_buffer].bol;

    [NativeTypeName("#define SCANtext_ready (SCANbuffer.numRead != 0)")]
    public static bool SCANtext_ready => (__SCAN_buffers[*__SCAN_current_buffer].numRead != 0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void LISTinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void LISTcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* LISTcreate();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* LISTcopy([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void LISTsort([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("int (*)(void *, void *)")] delegate* unmanaged[Cdecl]<void*, void*, int> comp);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void LISTswap([NativeTypeName("Link")] Link_* param0, [NativeTypeName("Link")] Link_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTadd_first([NativeTypeName("Linked_List")] Linked_List_* param0, void* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTadd_last([NativeTypeName("Linked_List")] Linked_List_* param0, void* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTadd_after([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("Link")] Link_* param1, void* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTadd_before([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("Link")] Link_* param1, void* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTremove_first([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTget_first([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTget_second([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* LISTget_nth([NativeTypeName("Linked_List")] Linked_List_* param0, int param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void LISTfree([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int LISTget_length([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool LISTempty([NativeTypeName("Linked_List")] Linked_List_* list);

    [NativeTypeName("#define LINK_NULL (Link)NULL")]
    public static Link_* LINK_NULL => (Link_*)((void*)(0));

    [NativeTypeName("#define LIST_NULL (Linked_List)NULL")]
    public static Linked_List_* LIST_NULL => (Linked_List_*)((void*)(0));

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void MEMORYinitialize();

    [NativeTypeName("#define MAX_OBJECT_TYPES 127")]
    public const int MAX_OBJECT_TYPES = 127;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void RESOLVEinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void RESOLVEcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPEresolve_expressions_statements([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPEresolve_subsupers([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPEresolve_types([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPE_resolve([NativeTypeName("Type *")] Scope_** param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXP_resolve([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Scope")] Scope_* param1, [NativeTypeName("Type")] Scope_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALGresolve([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAresolve([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void RENAMEresolve(Rename* param0, [NativeTypeName("Schema")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void VAR_resolve_expressions([NativeTypeName("Variable")] Variable_* param0, [NativeTypeName("Entity")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYresolve_subtypes([NativeTypeName("Schema")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYresolve_supertypes([NativeTypeName("Entity")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYresolve_expressions([NativeTypeName("Entity")] Scope_* e);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALGresolve_expressions_statements([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int WHEREresolve([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("Scope")] Scope_* param1, int param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEresolve_expressions([NativeTypeName("Type")] Scope_* param0, [NativeTypeName("Scope")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void STMTresolve([NativeTypeName("Statement")] Statement_* param0, [NativeTypeName("Scope")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void STMTlist_resolve([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("Scope")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int ENTITYresolve_subtype_expression([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Entity")] Scope_* param1, [NativeTypeName("Linked_List *")] Linked_List_** param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Variable")]
    public static extern Variable_* VARfind([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1, int param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Schema")]
    public static extern Scope_* SCHEMAcreate();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAadd_use([NativeTypeName("Schema")] Scope_* param0, [NativeTypeName("Symbol *")] Symbol_* param1, [NativeTypeName("Symbol *")] Symbol_* param2, [NativeTypeName("Symbol *")] Symbol_* param3);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAadd_reference([NativeTypeName("Schema")] Scope_* param0, [NativeTypeName("Symbol *")] Symbol_* param1, [NativeTypeName("Symbol *")] Symbol_* param2, [NativeTypeName("Symbol *")] Symbol_* param3);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAdefine_use([NativeTypeName("Schema")] Scope_* param0, Rename* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAdefine_reference([NativeTypeName("Schema")] Scope_* param0, Rename* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* SCHEMAfind([NativeTypeName("Schema")] Scope_* param0, [NativeTypeName("char *")] sbyte* name, int search_refs);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Scope")]
    public static extern Scope_* SCOPEcreate([NativeTypeName("char")] sbyte param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Scope")]
    public static extern Scope_* SCOPEcreate_tiny([NativeTypeName("char")] sbyte param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Scope")]
    public static extern Scope_* SCOPEcreate_nostab([NativeTypeName("char")] sbyte param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPEdestroy([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCHEMAget_entities_use([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCHEMAget_entities_ref([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMA_get_entities_ref([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [NativeTypeName("#define SCOPE_NULL (Scope)0")]
    public static Scope_* SCOPE_NULL => (Scope_*)(0);

    [NativeTypeName("#define SCOPE_FIND_TYPE OBJ_TYPE_BITS")]
    public const int SCOPE_FIND_TYPE = 0x1;

    [NativeTypeName("#define SCOPE_FIND_ENTITY OBJ_ENTITY_BITS")]
    public const int SCOPE_FIND_ENTITY = 0x2;

    [NativeTypeName("#define SCOPE_FIND_FUNCTION OBJ_FUNCTION_BITS")]
    public const int SCOPE_FIND_FUNCTION = 0x4;

    [NativeTypeName("#define SCOPE_FIND_PROCEDURE OBJ_PROCEDURE_BITS")]
    public const int SCOPE_FIND_PROCEDURE = 0x8;

    [NativeTypeName("#define SCOPE_FIND_VARIABLE OBJ_VARIABLE_BITS")]
    public const int SCOPE_FIND_VARIABLE = 0x100;

    [NativeTypeName("#define SCOPE_FIND_ANYTHING OBJ_ANYTHING_BITS")]
    public const int SCOPE_FIND_ANYTHING = 0x0fffffff;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("struct Symbol_ *")]
    public static extern Symbol_* SCOPE_get_symbol(void* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPE_get_entities([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCOPEget_entities([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCOPEget_entities_superclass_order([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* SCOPEfind([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1, int param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPE_get_functions([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCOPEget_functions([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCOPE_get_rules([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Linked_List")]
    public static extern Linked_List_* SCOPEget_rules([NativeTypeName("Scope")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* SCOPE_find([NativeTypeName("Scope")] Scope_* param0, [NativeTypeName("char *")] sbyte* param1, int param2);

    [NativeTypeName("#define STATEMENT_LIST_NULL (Linked_List)0")]
    public static Linked_List_* STATEMENT_LIST_NULL => (Linked_List_*)(0);

    [NativeTypeName("#define STMT_ASSIGN 0x1")]
    public const int STMT_ASSIGN = 0x1;

    [NativeTypeName("#define STMT_CASE 0x2")]
    public const int STMT_CASE = 0x2;

    [NativeTypeName("#define STMT_COMPOUND 0x4")]
    public const int STMT_COMPOUND = 0x4;

    [NativeTypeName("#define STMT_COND 0x8")]
    public const int STMT_COND = 0x8;

    [NativeTypeName("#define STMT_LOOP 0x10")]
    public const int STMT_LOOP = 0x10;

    [NativeTypeName("#define STMT_PCALL 0x20")]
    public const int STMT_PCALL = 0x20;

    [NativeTypeName("#define STMT_RETURN 0x40")]
    public const int STMT_RETURN = 0x40;

    [NativeTypeName("#define STMT_ALIAS 0x80")]
    public const int STMT_ALIAS = 0x80;

    [NativeTypeName("#define STMT_SKIP 0x100")]
    public const int STMT_SKIP = 0x100;

    [NativeTypeName("#define STMT_ESCAPE 0x200")]
    public const int STMT_ESCAPE = 0x200;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* STMTcreate(int param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* ALIAScreate([NativeTypeName("struct Scope_ *")] Scope_* param0, [NativeTypeName("Variable")] Variable_* param1, [NativeTypeName("Linked_List")] Linked_List_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* CASEcreate([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* ASSIGNcreate([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Expression")] Expression_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* COMP_STMTcreate([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* CONDcreate([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Linked_List")] Linked_List_* param1, [NativeTypeName("Linked_List")] Linked_List_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* LOOPcreate([NativeTypeName("struct Scope_ *")] Scope_* param0, [NativeTypeName("Expression")] Expression_* param1, [NativeTypeName("Expression")] Expression_* param2, [NativeTypeName("Linked_List")] Linked_List_* param3);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* PCALLcreate([NativeTypeName("Linked_List")] Linked_List_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Statement")]
    public static extern Statement_* RETcreate([NativeTypeName("Expression")] Expression_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void STMTinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("struct Scope_ *")]
    public static extern Scope_* INCR_CTLcreate([NativeTypeName("Symbol *")] Symbol_* param0, [NativeTypeName("Expression")] Expression_* start, [NativeTypeName("Expression")] Expression_* end, [NativeTypeName("Expression")] Expression_* increment);

    [NativeTypeName("#define STATEMENT_NULL (Statement)0")]
    public static Statement_* STATEMENT_NULL => (Statement_*)(0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SYMBOLinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Symbol *")]
    public static extern Symbol_* SYMBOLcreate([NativeTypeName("char *")] sbyte* name, int line, [NativeTypeName("const char *")] sbyte* filename);

    [NativeTypeName("#define TYPE_NULL (Type)0")]
    public static Scope_* TYPE_NULL => (Scope_*)(0);

    [NativeTypeName("#define AGGR_CHUNK_SIZE 30")]
    public const int AGGR_CHUNK_SIZE = 30;

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_partial([NativeTypeName("struct Symbol_ *")] Symbol_* param0, [NativeTypeName("Scope")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate([NativeTypeName("enum type_enum")] type_enum param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_from_body_anonymously([NativeTypeName("TypeBody")] TypeBody_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_name([NativeTypeName("struct Symbol_ *")] Symbol_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_nostab([NativeTypeName("struct Symbol_ *")] Symbol_* param0, [NativeTypeName("Scope")] Scope_* param1, [NativeTypeName("char")] sbyte param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("TypeBody")]
    public static extern TypeBody_* TYPEBODYcreate([NativeTypeName("enum type_enum")] type_enum param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEcleanup();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("_Bool")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool TYPEinherits_from([NativeTypeName("Type")] Scope_* param0, [NativeTypeName("enum type_enum")] type_enum param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEget_nonaggregate_base_type([NativeTypeName("Type")] Scope_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_user_defined_type([NativeTypeName("Type")] Scope_* param0, [NativeTypeName("Scope")] Scope_* param1, [NativeTypeName("struct Symbol_ *")] Symbol_* param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Type")]
    public static extern Scope_* TYPEcreate_user_defined_tag([NativeTypeName("Type")] Scope_* param0, [NativeTypeName("Scope")] Scope_* param1, [NativeTypeName("struct Symbol_ *")] Symbol_* param2);

    [NativeTypeName("#define CLASSinherits_from TYPEinherits_from")]
    public static delegate*<Scope_*, type_enum, bool> CLASSinherits_from => &TYPEinherits_from;

    [NativeTypeName("#define VARIABLE_NULL (Variable)0")]
    public static Variable_* VARIABLE_NULL => (Variable_*)(0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Variable")]
    public static extern Variable_* VARcreate([NativeTypeName("Expression")] Expression_* param0, [NativeTypeName("Type")] Scope_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void VARinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* VARget_simple_name([NativeTypeName("Variable")] Variable_* param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Scope")]
    public static extern Scope_* ALGcreate([NativeTypeName("char")] sbyte param0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALGinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALGput_full_text([NativeTypeName("Scope")] Scope_* param0, int param1, int param2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* nnew();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void _ALLOCinitialize();

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALLOCinitialize([NativeTypeName("struct freelist_head *")] freelist_head* flh, [NativeTypeName("unsigned int")] uint size, int alloc1, int alloc2);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ALLOC_destroy([NativeTypeName("struct freelist_head *")] freelist_head* param0, freelist* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void* ALLOC_new([NativeTypeName("struct freelist_head *")] freelist_head* param0);

    [NativeTypeName("#define CASE_ITEM_NULL (struct Case_Item_ *)0")]
    public static Case_Item_* CASE_ITEM_NULL => (Case_Item_*)(0);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("Case_Item")]
    public static extern Case_Item_* CASE_ITcreate([NativeTypeName("Linked_List")] Linked_List_* param0, [NativeTypeName("struct Statement_ *")] Statement_* param1);

    [DllImport("express", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void CASE_ITinitialize();
}
