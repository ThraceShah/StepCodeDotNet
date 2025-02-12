using System.Runtime.ConstrainedExecution;
using System.Text;
using StepCodeDotNet;

namespace StepCodeDotNet;
public enum Boolean { BFalse, BTrue, BUnset };
public enum Logical { LFalse, LTrue, LUnset, LUnknown };
public abstract unsafe class SDAI_Enum
{
    protected int v = 0;

    protected virtual int set_value(string? n)
    {
        if (string.IsNullOrEmpty(n))
        {
            nullify();
            return asInt();
        }
        int i = 0;
        while ((i < no_elements()) &&
                (string.Compare(n, element_at(i), true) != 0))
        {
            ++i;
        }
        if (no_elements() == i)
        {
            //  exhausted all the possible values
            return v = no_elements() + 1; // defined as UNSET
        }
        v = i;
        return v;
    }
    protected virtual int set_value(int i)
    {
        if (i > no_elements())
        {
            v = no_elements() + 1;
            return v;
        }
        if (element_at(i)[0] != '\0')
        {
            return v = i;
        }
        Console.WriteLine("(OLD Warning:) invalid enumeration value {0} for {1}", i, Name());
        DebugDisplay();
        return no_elements() + 1;
    }

    public void PrintContents(Stream? _out = null)
    {
        _out ??= Console.OpenStandardOutput();
        DebugDisplay(_out);
    }

    public abstract int no_elements();
    public abstract string Name();
    public string get_value_at(int n) => element_at(n);
    public abstract string element_at(int n);
    public Severity EnumValidLevel(sbyte* value, ErrorDescriptor err,
                             int optional, sbyte* tokenList,
                             int needDelims = 0, int clearError = 1)
    {
        var reader = new SDAIStream<sbyte>(value);
        if (clearError != 0)
        {
            err.ClearErrorMsg();
        }
        reader.SkipWhiteSpace();
        sbyte c = reader.Peek();
        if (c == '$' || reader.EndOfStream())
        {
            if (optional == 0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            if (!reader.EndOfStream())
            {
                reader.SkipWhiteSpace();
                reader.Read();
            }
            reader.CheckRemainingInput(err, "enumeration", tokenList);
            return err.severity();
        }
        else
        {
            ErrorDescriptor error = new();
            ReadEnum(reader, error, 0, needDelims);
            reader.CheckRemainingInput(error, "enumeration", tokenList);

            Severity sev = error.severity();
            if (sev < Severity.SEVERITY_INCOMPLETE)
            {
                err.AppendToDetailMsg(error.DetailMsg());
                err.AppendToUserMsg(error.UserMsg());
                err.GreaterSeverity(error.severity());
            }
            else if (sev == Severity.SEVERITY_INCOMPLETE && optional == 0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
        }
        return err.severity();
    }

    public Severity EnumValidLevel(StreamReader reader, ErrorDescriptor err,
                             int optional, char* tokenList,
                             int needDelims = 0, int clearError = 1)
    {
        if (clearError != 0)
        {
            err.ClearErrorMsg();
        }
        reader.SkipWhiteSpace();
        char c = (char)reader.Peek();
        if (c == '$' || reader.EndOfStream)
        {
            if (optional == 0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            if (!reader.EndOfStream)
            {
                reader.SkipWhiteSpace();
                reader.Read();
            }
            reader.CheckRemainingInput(err, "enumeration", tokenList);
            return err.severity();
        }
        else
        {
            ErrorDescriptor error = new();
            ReadEnum(reader, error, 0, needDelims);
            reader.CheckRemainingInput(error, "enumeration", tokenList);

            Severity sev = error.severity();
            if (sev < Severity.SEVERITY_INCOMPLETE)
            {
                err.AppendToDetailMsg(error.DetailMsg());
                err.AppendToUserMsg(error.UserMsg());
                err.GreaterSeverity(error.severity());
            }
            else if (sev == Severity.SEVERITY_INCOMPLETE && optional == 0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
        }
        return err.severity();
    }

    public int asInt() => v;

    public string asStr(string s)
    {
        if (exists())
        {
            s = element_at(v);
            return s;
        }
        else
        {
            return "";
        }
    }
    public void STEPwrite(Stream outStream)
    {
        using StreamWriter writer = new(outStream);
        if (is_null())
        {
            writer.Write('$');
        }
        else
        {
            string tmp = string.Empty;
            writer.Write($".{asStr(tmp)}.");
        }
    }
    public string STEPwrite(string s)
    {
        if (is_null())
        {
            s = string.Empty;
        }
        else
        {
            s = $".{asStr("")}.";
        }
        return s;
    }

    public Severity StrToVal(sbyte* s, ErrorDescriptor err, int optional = 1)
    {
        ReadEnum(s, err, 1, 0);
        if ((err.severity() == Severity.SEVERITY_INCOMPLETE) && optional != 0)
        {
            err.severity(Severity.SEVERITY_NULL);
        }
        return err.severity();
    }
    public Severity STEPread(sbyte* s, ErrorDescriptor err, int optional = 1)
    {
        return ReadEnum(s, err, 1, 0);
    }
    public Severity STEPread(StreamReader s, ErrorDescriptor err, int optional = 1)
    {
        return ReadEnum(s, err, 1, 0);
    }

    public virtual int put(int val) => set_value(val);
    public virtual int put(string n) => set_value(n);
    public bool is_null() => exists() == false;
    public void set_null() => nullify();

    /// WARNING it appears that exists() will return true after a call to nullify(). is this intended?
    ///FIXME need to rewrite this function, but strange implementation...
    public virtual bool exists() => !(v > no_elements());
    public virtual void nullify() => set_value(no_elements() + 1);
    public void DebugDisplay(Stream? outStream = null)
    {
        outStream ??= Console.OpenStandardOutput();
        using StreamWriter writer = new(outStream);
        writer.WriteLine($"Current {Name()} value:");
        writer.WriteLine($"  cardinal: {v}");
        writer.WriteLine($"  string: {asStr("")}");
        writer.Write("  Part 21 file format: ");
        STEPwrite(outStream);
        writer.WriteLine();
        writer.WriteLine("Valid values are:");
        int i = 0;
        while (i < no_elements() + 1)
        {
            writer.WriteLine($"{i} {element_at(i)}");
            i++;
        }
        writer.WriteLine();
    }

    protected virtual Severity ReadEnum(StreamReader inReader, ErrorDescriptor err,
                                       int AssignVal = 1, int needDelims = 1)
    {
        if (AssignVal != 0)
        {
            set_null();
        }

        string str = string.Empty;
        char[] messageBuf = new char[512];

        // Skip white space
        while (inReader.Peek() != -1 && char.IsWhiteSpace((char)inReader.Peek()))
        {
            inReader.Read();
        }

        if (inReader.Peek() != -1)
        {
            char c = (char)inReader.Read();
            if (c == '.' || char.IsLetter(c))
            {
                int validDelimiters = 1;
                if (c == '.')
                {
                    c = (char)inReader.Read(); // push past the delimiter
                                               // since found a valid delimiter it is now invalid until the
                                               // matching ending delim is found
                    validDelimiters = 0;
                }

                // look for UPPER
                if (inReader.Peek() != -1 && (char.IsLetter(c) || c == '_'))
                {
                    str += c;
                    c = (char)inReader.Read();
                }

                // look for UPPER or DIGIT
                while (inReader.Peek() != -1 && (char.IsLetterOrDigit(c) || c == '_'))
                {
                    str += c;
                    c = (char)inReader.Read();
                }

                // if character is not the delimiter unread it
                if (inReader.Peek() != -1 && c != '.')
                {
                    inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                }

                // a value was read
                if (str.Length > 0)
                {
                    int i = 0;
                    string strval = str.ToUpper();
                    while (i < no_elements() && string.Compare(strval, element_at(i), StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        ++i;
                    }
                    if (no_elements() == i)
                    {
                        // exhausted all the possible values
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        err.AppendToDetailMsg("Invalid Enumeration value.\n");
                        err.AppendToUserMsg("Invalid Enumeration value.\n");
                    }
                    else
                    {
                        if (AssignVal != 0)
                        {
                            v = i;
                        }
                    }

                    // now also check the delimiter situation
                    if (c == '.')
                    {
                        // if found ending delimiter
                        // if expecting delim (i.e. validDelimiter == 0)
                        if (validDelimiters == 0)
                        {
                            validDelimiters = 1; // everything is fine
                        }
                        else
                        {
                            // found ending delimiter but no initial delimiter
                            validDelimiters = 0;
                        }
                    }
                    // didn't find any delimiters at all and need them.
                    else if (needDelims != 0)
                    {
                        validDelimiters = 0;
                    }

                    if (validDelimiters == 0)
                    {
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        if (needDelims != 0)
                        {
                            messageBuf = "Enumerated value has invalid period delimiters.\n".ToCharArray();
                        }
                        else
                        {
                            messageBuf = "Mismatched period delimiters for enumeration.\n".ToCharArray();
                        }
                        err.AppendToDetailMsg(new string(messageBuf));
                        err.AppendToUserMsg(new string(messageBuf));
                    }
                    return err.severity();
                }
                // found valid or invalid delimiters with no associated value
                else if (c == '.' || validDelimiters == 0)
                {
                    err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    err.AppendToDetailMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    err.AppendToUserMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    return err.severity();
                }
                else
                {
                    // no delims and no value
                    err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
                }
            }
            else if (c == ',' || c == ')')
            {
                inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            else
            {
                inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                messageBuf = "Invalid enumeration value.\n".ToCharArray();
                err.AppendToDetailMsg(new string(messageBuf));
                err.AppendToUserMsg(new string(messageBuf));
            }
        }
        else
        {
            // hit eof (assuming there was no error state for istream passed in)
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }
    protected virtual Severity ReadEnum(sbyte* inStr, ErrorDescriptor err,
                                       int AssignVal = 1, int needDelims = 1)
    {
        var reader = new SDAIStream<sbyte>(inStr);
        return ReadEnum(reader, err, AssignVal, needDelims);
    }
    protected virtual Severity ReadEnum(SDAIStream<sbyte> inReader, ErrorDescriptor err,
                                       int AssignVal = 1, int needDelims = 1)
    {
        if (AssignVal != 0)
        {
            set_null();
        }
        string str = string.Empty;
        string messageBuf = string.Empty;
        // Skip white space
        while (inReader.Peek() != 0 && char.IsWhiteSpace((char)inReader.Peek()))
        {
            inReader.Read();
        }

        if (inReader.Peek() != 0)
        {
            sbyte c = inReader.Read();
            if (c == '.' || c.IsAlpha())
            {
                int validDelimiters = 1;
                if (c == '.')
                {
                    c = inReader.Read(); // push past the delimiter
                                         // since found a valid delimiter it is now invalid until the
                                         // matching ending delim is found
                    validDelimiters = 0;
                }

                // look for UPPER
                if (inReader.Peek() != 0 && (c.IsAlpha() || c == '_'))
                {
                    str += c;
                    c = inReader.Read();
                }

                // look for UPPER or DIGIT
                while (inReader.Peek() != 0 && (c.IsAlNum() || c == '_'))
                {
                    str += c;
                    c = inReader.Read();
                }

                // if character is not the delimiter unread it
                if (inReader.Peek() != 0 && c != '.')
                {
                    inReader.Seek(-1);
                }

                // a value was read
                if (str.Length > 0)
                {
                    int i = 0;
                    string strval = str.ToUpper();
                    while (i < no_elements() && string.Compare(strval, element_at(i), StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        ++i;
                    }
                    if (no_elements() == i)
                    {
                        // exhausted all the possible values
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        err.AppendToDetailMsg("Invalid Enumeration value.\n");
                        err.AppendToUserMsg("Invalid Enumeration value.\n");
                    }
                    else
                    {
                        if (AssignVal != 0)
                        {
                            v = i;
                        }
                    }

                    // now also check the delimiter situation
                    if (c == '.')
                    {
                        // if found ending delimiter
                        // if expecting delim (i.e. validDelimiter == 0)
                        if (validDelimiters == 0)
                        {
                            validDelimiters = 1; // everything is fine
                        }
                        else
                        {
                            // found ending delimiter but no initial delimiter
                            validDelimiters = 0;
                        }
                    }
                    // didn't find any delimiters at all and need them.
                    else if (needDelims != 0)
                    {
                        validDelimiters = 0;
                    }

                    if (validDelimiters == 0)
                    {
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        if (needDelims != 0)
                        {
                            messageBuf = "Enumerated value has invalid period delimiters.\n";
                        }
                        else
                        {
                            messageBuf = "Mismatched period delimiters for enumeration.\n";
                        }
                        err.AppendToDetailMsg(messageBuf);
                        err.AppendToUserMsg(messageBuf);
                    }
                    return err.severity();
                }
                // found valid or invalid delimiters with no associated value
                else if (c == '.' || validDelimiters == 0)
                {
                    err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    err.AppendToDetailMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    err.AppendToUserMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    return err.severity();
                }
                else
                {
                    // no delims and no value
                    err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
                }
            }
            else if (c == ',' || c == ')')
            {
                inReader.Seek(-1);
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            else
            {
                inReader.Seek(-1);
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                messageBuf = "Invalid enumeration value.\n";
                err.AppendToDetailMsg(messageBuf);
                err.AppendToUserMsg(messageBuf);
            }
        }
        else
        {
            // hit eof (assuming there was no error state for istream passed in)
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }

}

public unsafe class SDAI_LOGICAL : SDAI_Enum
{
    public override string Name() => "Logical";
    public SDAI_LOGICAL(string val="")
    {
        set_value(val);
    }
    public SDAI_LOGICAL(Logical state)
    {
        set_value((int)state);
    }
    public SDAI_LOGICAL(int i)
    {
        set_value(i);
    }
    public static implicit operator Logical(SDAI_LOGICAL s) => (Logical)s.v switch
    {
        Logical.LFalse => Logical.LFalse,
        Logical.LTrue => Logical.LTrue,
        Logical.LUnknown => Logical.LUnknown,
        _ => Logical.LUnset,
    };
    public static implicit operator SDAI_LOGICAL(Logical s) => new(s);
    public static bool operator ==(SDAI_LOGICAL a, Logical b) => a.v == (int)b;
    public static bool operator !=(SDAI_LOGICAL a, Logical b) => a.v != (int)b;
    public static bool operator true(SDAI_LOGICAL a) => a.v == 1;
    public static bool operator false(SDAI_LOGICAL a) => a.v == 0;
    public override int no_elements() => 3;
    public override string element_at(int n) => (Logical)n switch
    {
        Logical.LUnknown => "U",
        Logical.LFalse => "F",
        Logical.LTrue => "T",
        _ => "UNSET",
    };
    public new bool exists() => !(v == 2);
    public new void nullify() => v=2;
    protected override int set_value(int i)
    {
        if (i > no_elements() + 1)
        {
            v = 2;
            return v;
        }
        string tmp = element_at(i);
        if (tmp[0] != '\0')
        {
            return (v = i);
        }
        // otherwise
        Console.WriteLine("(OLD Warning:) invalid enumeration value {0} for {1}", i, Name());
        DebugDisplay();
        return no_elements() + 1;
    }
    protected override int set_value(string? n)
    {
        if (string.IsNullOrEmpty(n))
        {
            nullify();
            return asInt();
        }
        int i = 0;
        while ((i < no_elements()+1) &&
                (string.Compare(n, element_at(i), true) != 0))
        {
            ++i;
        }
        if (no_elements()+1 == i)
        {
            nullify();
            return v;
        }
        v = i;
        return v;
    }
    protected override Severity ReadEnum(StreamReader inReader, ErrorDescriptor err,
                                int AssignVal = 1, int needDelims = 1)
    {
        if (AssignVal != 0)
        {
            set_null();
        }

        string str = string.Empty;

        inReader.SkipWhiteSpace();

        if (inReader.Peek() != -1)
        {
            char c = (char)inReader.Read();
            if (c == '.' || char.IsLetter(c))
            {
                int validDelimiters = 1;
                if (c == '.')
                {
                    c = (char)inReader.Read(); // push past the delimiter
                                               // since found a valid delimiter it is now invalid until the
                                               // matching ending delim is found
                    validDelimiters = 0;
                }

                // look for UPPER
                if (inReader.Peek() != -1 && (char.IsLetter(c) || c == '_'))
                {
                    str += c;
                    c = (char)inReader.Read();
                }

                // look for UPPER or DIGIT
                while (inReader.Peek() != -1 && (char.IsLetterOrDigit(c) || c == '_'))
                {
                    str += c;
                    c = (char)inReader.Read();
                }

                // if character is not the delimiter unread it
                if (inReader.Peek() != -1 && c != '.')
                {
                    inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                }

                // a value was read
                if (str.Length > 0)
                {
                    int i = 0;
                    while (i < no_elements()+1 && string.Compare(str, element_at(i), StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        ++i;
                    }
                    if (no_elements()+1 == i)
                    {
                        // exhausted all the possible values
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        err.AppendToDetailMsg("Invalid Enumeration value.\n");
                        err.AppendToUserMsg("Invalid Enumeration value.\n");
                    }
                    else
                    {
                        if (AssignVal != 0)
                        {
                            v = i;
                        }
                    }

                    // now also check the delimiter situation
                    if (c == '.')
                    {
                        // if found ending delimiter
                        // if expecting delim (i.e. validDelimiter == 0)
                        if (validDelimiters == 0)
                        {
                            validDelimiters = 1; // everything is fine
                        }
                        else
                        {
                            // found ending delimiter but no initial delimiter
                            validDelimiters = 0;
                        }
                    }
                    // didn't find any delimiters at all and need them.
                    else if (needDelims != 0)
                    {
                        validDelimiters = 0;
                    }

                    if (validDelimiters == 0)
                    {
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        var messageBuf=string.Empty;
                        if (needDelims != 0)
                        {
                            messageBuf = "Enumerated value has invalid period delimiters.\n";
                        }
                        else
                        {
                            messageBuf = "Mismatched period delimiters for enumeration.\n";
                        }
                        err.AppendToDetailMsg(messageBuf);
                        err.AppendToUserMsg(messageBuf);
                    }
                    return err.severity();
                }
                // found valid or invalid delimiters with no associated value
                else if (c == '.' || validDelimiters == 0)
                {
                    err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    err.AppendToDetailMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    err.AppendToUserMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    return err.severity();
                }
                else
                {
                    // no delims and no value
                    err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
                }
            }
            else if (c == ',' || c == ')')
            {
                inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            else
            {
                inReader.BaseStream.Seek(-1, SeekOrigin.Current);
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                var messageBuf = "Invalid enumeration value.\n";
                err.AppendToDetailMsg(messageBuf);
                err.AppendToUserMsg(messageBuf);
            }
        }
        else
        {
            // hit eof (assuming there was no error state for istream passed in)
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }
    protected override Severity ReadEnum(sbyte* inStr, ErrorDescriptor err,
                                   int AssignVal = 1, int needDelims = 1)
    {
        var reader = new SDAIStream<sbyte>(inStr);
        return ReadEnum(reader, err, AssignVal, needDelims);
    }
    protected override Severity ReadEnum(SDAIStream<sbyte> inReader, ErrorDescriptor err,
                                       int AssignVal = 1, int needDelims = 1)
    {
        if (AssignVal != 0)
        {
            set_null();
        }
        string str = string.Empty;
        string messageBuf = string.Empty;
        // Skip white space
        while (inReader.Peek() != 0 && char.IsWhiteSpace((char)inReader.Peek()))
        {
            inReader.Read();
        }

        if (inReader.Peek() != 0)
        {
            sbyte c = inReader.Read();
            if (c == '.' || c.IsAlpha())
            {
                int validDelimiters = 1;
                if (c == '.')
                {
                    c = inReader.Read(); // push past the delimiter
                                         // since found a valid delimiter it is now invalid until the
                                         // matching ending delim is found
                    validDelimiters = 0;
                }

                // look for UPPER
                if (inReader.Peek() != 0 && (c.IsAlpha() || c == '_'))
                {
                    str += c;
                    c = inReader.Read();
                }

                // look for UPPER or DIGIT
                while (inReader.Peek() != 0 && (c.IsAlNum() || c == '_'))
                {
                    str += c;
                    c = inReader.Read();
                }

                // if character is not the delimiter unread it
                if (inReader.Peek() != 0 && c != '.')
                {
                    inReader.Seek(-1);
                }

                // a value was read
                if (str.Length > 0)
                {
                    int i = 0;
                    string strval = str.ToUpper();
                    while (i < no_elements()+1 && string.Compare(strval, element_at(i), StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        ++i;
                    }
                    if (no_elements()+1 == i)
                    {
                        // exhausted all the possible values
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        err.AppendToDetailMsg("Invalid Enumeration value.\n");
                        err.AppendToUserMsg("Invalid Enumeration value.\n");
                    }
                    else
                    {
                        if (AssignVal != 0)
                        {
                            v = i;
                        }
                    }

                    // now also check the delimiter situation
                    if (c == '.')
                    {
                        // if found ending delimiter
                        // if expecting delim (i.e. validDelimiter == 0)
                        if (validDelimiters == 0)
                        {
                            validDelimiters = 1; // everything is fine
                        }
                        else
                        {
                            // found ending delimiter but no initial delimiter
                            validDelimiters = 0;
                        }
                    }
                    // didn't find any delimiters at all and need them.
                    else if (needDelims != 0)
                    {
                        validDelimiters = 0;
                    }

                    if (validDelimiters == 0)
                    {
                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                        if (needDelims != 0)
                        {
                            messageBuf = "Enumerated value has invalid period delimiters.\n";
                        }
                        else
                        {
                            messageBuf = "Mismatched period delimiters for enumeration.\n";
                        }
                        err.AppendToDetailMsg(messageBuf);
                        err.AppendToUserMsg(messageBuf);
                    }
                    return err.severity();
                }
                // found valid or invalid delimiters with no associated value
                else if (c == '.' || validDelimiters == 0)
                {
                    err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    err.AppendToDetailMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    err.AppendToUserMsg("Enumerated has valid or invalid period delimiters with no value.\n");
                    return err.severity();
                }
                else
                {
                    // no delims and no value
                    err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
                }
            }
            else if (c == ',' || c == ')')
            {
                inReader.Seek(-1);
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            else
            {
                inReader.Seek(-1);
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                messageBuf = "Invalid enumeration value.\n";
                err.AppendToDetailMsg(messageBuf);
                err.AppendToUserMsg(messageBuf);
            }
        }
        else
        {
            // hit eof (assuming there was no error state for istream passed in)
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }


}

public unsafe class SDAI_BOOLEAN:SDAI_Enum
{
    public override string Name() => "Bool";
    public SDAI_BOOLEAN(string val = "")
    {
        set_value(val);
    }
    public SDAI_BOOLEAN(Boolean state)
    {
        set_value((int)state);
    }
    public SDAI_BOOLEAN(int i)
    {
        if (i == 0)
        {
            v = 0;
        }
        else
        {
            v = 1;
        }
    }
    public SDAI_BOOLEAN(SDAI_LOGICAL val)
    {
        set_value(val.asInt());
    }
    public override int no_elements() => 2;
    public override string element_at(int n) => (Boolean)n switch
    {
        Boolean.BFalse => "F",
        Boolean.BTrue => "T",
        _ => "UNSET",
    };

    public static implicit operator Boolean(SDAI_BOOLEAN s) => (Boolean)s.v switch
    {
        Boolean.BFalse => Boolean.BFalse,
        Boolean.BTrue => Boolean.BTrue,
        _ => Boolean.BUnset,
    };
    public static implicit operator SDAI_BOOLEAN(Boolean s) => new(s);
    public static implicit operator SDAI_BOOLEAN(SDAI_LOGICAL s) => new(s.asInt());
    public static implicit operator SDAI_LOGICAL(SDAI_BOOLEAN s) => new(s.v);
    public static bool operator ==(SDAI_BOOLEAN a, Boolean b) => a.v == (int)b;
    public static bool operator !=(SDAI_BOOLEAN a, Boolean b) => a.v != (int)b;
    public static bool operator true(SDAI_BOOLEAN a) => a.v == 1;
    public static bool operator false(SDAI_BOOLEAN a) => a.v == 0;

}