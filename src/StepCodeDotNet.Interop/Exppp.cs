using System.Runtime.InteropServices;

namespace StepCodeDotNet.Interop;

public static unsafe partial class Exppp
{
    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRESSout([NativeTypeName("Express")] Scope_* e);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ENTITYout([NativeTypeName("Entity")] Scope_* e);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void EXPRout([NativeTypeName("Expression")] Expression_* expr);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void FUNCout([NativeTypeName("Function")] Scope_* f);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void PROCout([NativeTypeName("Procedure")] Scope_* p);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void RULEout([NativeTypeName("Rule")] Scope_* r);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* SCHEMAout([NativeTypeName("Schema")] Scope_* s);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void SCHEMAref_out([NativeTypeName("Schema")] Scope_* s);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void STMTout([NativeTypeName("Statement")] Statement_* s);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEout([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEhead_out([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void TYPEbody_out([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void WHEREout([NativeTypeName("Linked_List")] Linked_List_* w);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* REFto_string([NativeTypeName("Dictionary")] Hash_Table_* refdict, [NativeTypeName("Linked_List")] Linked_List_* reflist, [NativeTypeName("char *")] sbyte* type, int level);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* ENTITYto_string([NativeTypeName("Entity")] Scope_* e);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* SUBTYPEto_string([NativeTypeName("Expression")] Expression_* e);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* EXPRto_string([NativeTypeName("Expression")] Expression_* expr);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* FUNCto_string([NativeTypeName("Function")] Scope_* f);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* PROCto_string([NativeTypeName("Procedure")] Scope_* p);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* RULEto_string([NativeTypeName("Rule")] Scope_* r);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* SCHEMAref_to_string([NativeTypeName("Schema")] Scope_* s);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* STMTto_string([NativeTypeName("Statement")] Statement_* s);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* TYPEto_string([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* TYPEhead_to_string([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* TYPEbody_to_string([NativeTypeName("Type")] Scope_* t);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("char *")]
    public static extern sbyte* WHEREto_string([NativeTypeName("Linked_List")] Linked_List_* w);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int REFto_buffer([NativeTypeName("Dictionary")] Hash_Table_* refdict, [NativeTypeName("Linked_List")] Linked_List_* reflist, [NativeTypeName("char *")] sbyte* type, int level, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int ENTITYto_buffer([NativeTypeName("Entity")] Scope_* e, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int EXPRto_buffer([NativeTypeName("Expression")] Expression_* e, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int FUNCto_buffer([NativeTypeName("Function")] Scope_* e, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PROCto_buffer([NativeTypeName("Procedure")] Scope_* e, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int RULEto_buffer([NativeTypeName("Rule")] Scope_* e, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int SCHEMAref_to_buffer([NativeTypeName("Schema")] Scope_* s, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int STMTto_buffer([NativeTypeName("Statement")] Statement_* s, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int TYPEto_buffer([NativeTypeName("Type")] Scope_* t, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int TYPEhead_to_buffer([NativeTypeName("Type")] Scope_* t, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int TYPEbody_to_buffer([NativeTypeName("Type")] Scope_* t, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int WHEREto_buffer([NativeTypeName("Linked_List")] Linked_List_* w, [NativeTypeName("char *")] sbyte* buffer, int length);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int EXPRlength([NativeTypeName("Expression")] Expression_* e);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void tail_comment([NativeTypeName("const char *")] sbyte* name);

    [DllImport("exppp", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int count_newlines([NativeTypeName("char *")] sbyte* s);
}
