using System.Runtime.InteropServices;

namespace StepCodeDotNet.Interop;

public partial struct Statement_
{
    [NativeTypeName("Symbol")]
    public Symbol_ symbol;

    public int type;

    [NativeTypeName("union u_statement")]
    public u_statement u;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct u_statement
    {
        [FieldOffset(0)]
        [NativeTypeName("struct Alias_ *")]
        public Alias_* alias;

        [FieldOffset(0)]
        [NativeTypeName("struct Assignment_ *")]
        public Assignment_* assign;

        [FieldOffset(0)]
        [NativeTypeName("struct Case_Statement_ *")]
        public Case_Statement_* Case;

        [FieldOffset(0)]
        [NativeTypeName("struct Compound_Statement_ *")]
        public Compound_Statement_* compound;

        [FieldOffset(0)]
        [NativeTypeName("struct Conditional_ *")]
        public Conditional_* cond;

        [FieldOffset(0)]
        [NativeTypeName("struct Loop_ *")]
        public Loop_* loop;

        [FieldOffset(0)]
        [NativeTypeName("struct Procedure_Call_ *")]
        public Procedure_Call_* proc;

        [FieldOffset(0)]
        [NativeTypeName("struct Return_Statement_ *")]
        public Return_Statement_* ret;
    }
}
