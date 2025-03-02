namespace StepCodeDotNet.Interop;
public unsafe partial struct Symbol_
{
    [NativeTypeName("char *")]
    public sbyte* name;

    public readonly string Name
    {
        get
        {
            var str = new string(name);
            if (SymbolEX.IsKeywordKind(str))
            {
                return "@" + str;
            }
            return str;
        }
    }

    [NativeTypeName("const char *")]
    public sbyte* filename;

    public readonly string Filename => new(filename);

    public int line;

    [NativeTypeName("char")]
    public sbyte resolved;
}


static class SymbolEX
{
    private static readonly HashSet<string> Keywords =

    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
        "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while",

        "add", "alias", "ascending", "async", "await", "descending", "dynamic", "from", "get",
        "global", "group", "into", "join", "let", "nameof", "orderby", "partial", "remove",
        "select", "set", "unmanaged", "value", "var", "when", "where", "yield"
    ];

    public static bool IsKeywordKind(string name)
    {
        return Keywords.Contains(name);
    }
}