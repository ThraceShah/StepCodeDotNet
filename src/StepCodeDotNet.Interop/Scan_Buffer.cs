using System.Runtime.CompilerServices;

namespace StepCodeDotNet.Interop;

public unsafe partial struct Scan_Buffer
{
    [NativeTypeName("char[1025]")]
    public _text_e__FixedBuffer text;

    public int numRead;

    [NativeTypeName("char *")]
    public sbyte* savedPos;

    [NativeTypeName("FILE *")]
    public _iobuf* file;

    [NativeTypeName("const char *")]
    public sbyte* filename;

    [NativeTypeName("_Bool")]
    public bool readEof;

    public int lineno;

    public int bol;

    [InlineArray(1025)]
    public partial struct _text_e__FixedBuffer
    {
        public sbyte e0;
    }
}
