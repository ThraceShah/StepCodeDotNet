using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;

public unsafe class SDAI_Binary
{
    private StringBuilder content = new();

    public SDAI_Binary(sbyte* str = null, int max = 0)
    {
        for (int i = 0; i < max; i++)
        {
            content.Append((char)str[i]);
        }
    }

    public SDAI_Binary(string str)
    {
        content.Append(str);
    }

    public SDAI_Binary(sbyte* str)
    {
        while (*str != 0)
        {
            content.Append((char)*str);
            str++;
        }
    }

    public static implicit operator SDAI_Binary(string str)
    {
        return new SDAI_Binary(str);
    }

    public static implicit operator SDAI_Binary(sbyte* str)
    {
        return new SDAI_Binary(str);
    }

    public void clear()
    {
        content.Clear();
    }

    public bool empty()
    {
        return content.Length == 0;
    }

    public string c_str()
    {
        return content.ToString();
    }

    public string asStr()
    {
        return content.ToString();
    }

    public void STEPwrite(Stream? stream = null)
    {
        stream ??= Console.OpenStandardOutput();
        using var writer = new StreamWriter(stream);
        if(empty())
        {
            writer.Write("$");
        }
        else
        {
            writer.Write('\"');
            writer.Write(content.ToString());
            writer.Write('\"');
        }
    }

    public string STEPWrite()
    {
        if (empty())
        {
            return "$";
        }
        StringBuilder sb = new();
        sb.Append('\"');
        sb.Append(content);
        sb.Append('\"');
        return sb.ToString();
    }

    public Severity StrToVal(sbyte* s, ErrorDescriptor err)
    {
        var stream=new SDAIStream<sbyte>(s);
        return ReadBinary(stream, err,1,0);
    }

    public Severity STEPread(StreamReader stream, ErrorDescriptor err)
    {
        return ReadBinary(stream, err,1,1);
    }

    public Severity STEPread(sbyte* s, ErrorDescriptor err)
    {
        var stream = new SDAIStream<sbyte>(s);
        return ReadBinary(stream, err,1,1);

    }

    public Severity STEPread(SDAIStream<sbyte> stream, ErrorDescriptor err)
    {
        return ReadBinary(stream, err, 1, 1);
    }

    public Severity BinaryValidLevel( sbyte* value, ErrorDescriptor err,
                                   int optional, sbyte* tokenList,
                                   int needDelims = 0, int clearError = 1 )
    {
        var reader = new SDAIStream<sbyte>(value);
        if (clearError != 0)
        {
            err.ClearErrorMsg();
        }

        reader.SkipWhiteSpace();
        char c = (char)reader.Peek();
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
            reader.CheckRemainingInput(err, "binary", tokenList);
            return err.severity();
        }
        else
        {
            ErrorDescriptor error = new();
            ReadBinary(reader, error, 0, needDelims);
            reader.CheckRemainingInput(error, "binary", tokenList);

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

    public Severity BinaryValidLevel(StreamReader reader, ErrorDescriptor err,
                                   int optional, char* tokenList,
                                   int needDelims = 0, int clearError = 1)
    {
        if (clearError!=0)
        {
            err.ClearErrorMsg();
        }

        reader.SkipWhiteSpace();
        char c = (char)reader.Peek();
        if (c == '$' || reader.EndOfStream ) {
            if (optional==0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
            if (!reader.EndOfStream)
            {
                reader.SkipWhiteSpace();
                reader.Read();
            }
            reader.CheckRemainingInput(err, "binary", tokenList);
            return err.severity();
        } else
        {
            ErrorDescriptor error=new ();
            ReadBinary(reader, error, 0, needDelims);
            reader.CheckRemainingInput(error, "binary", tokenList);

            Severity sev = error.severity();
            if (sev < Severity.SEVERITY_INCOMPLETE)
            {
                err.AppendToDetailMsg(error.DetailMsg());
                err.AppendToUserMsg(error.UserMsg());
                err.GreaterSeverity(error.severity());
            }
            else if (sev == Severity.SEVERITY_INCOMPLETE && optional==0)
            {
                err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
            }
        }
        return err.severity();

    }

    protected Severity ReadBinary(StreamReader reader, ErrorDescriptor err, int AssignVal_ = 1,
                             int needDelims = 1)
    {
        bool AssignVal = AssignVal_ != 0;
        if (AssignVal)
        {
            clear();
        }

        string str = string.Empty;
        string messageBuf = string.Empty;

        reader.SkipWhiteSpace();
        ;
        if (reader.Peek() != 0)
        {
            var c = (char)reader.Read();
            if ((c == '\"') || char.IsAsciiHexDigit(c))
            {
                int validDelimiters = 1;
                if (c == '\"')
                {
                    c = (char)reader.Read(); // push past the delimiter
                                       // since found a valid delimiter it is now invalid until the
                                       //   matching ending delim is found
                    validDelimiters = 0;
                }
                while (reader.Peek() != 0 && char.IsAsciiHexDigit(c))
                {
                    str += c;
                    c = (char)reader.Read();
                }
                if (reader.Peek() != 0 && (c != '\"'))
                {
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                }
                if (AssignVal && (str.Length > 0))
                {
                    content.Clear();
                    content.Append(str);
                }

                if (c == '\"')
                { // if found ending delimiter
                  // if expecting delim (i.e. validDelimiter == 0)
                    if (validDelimiters == 0)
                    {
                        validDelimiters = 1; // everything is fine
                    }
                    else
                    { // found ending delimiter but no initial delimiter
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
                        messageBuf = "Binary value missing double quote delimiters.\n";
                    else
                        messageBuf = "Mismatched double quote delimiters for binary.\n";
                    err.AppendToDetailMsg(messageBuf);
                    err.AppendToUserMsg(messageBuf);
                }
            }
            else
            {
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                messageBuf = "Invalid binary value.\n";
                err.AppendToDetailMsg(messageBuf);
                err.AppendToUserMsg(messageBuf);
            }
        }
        else
        {
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }

    protected Severity ReadBinary(SDAIStream<sbyte> reader, ErrorDescriptor err, int AssignVal_ = 1,
                             int needDelims = 1)
    {
        bool AssignVal = AssignVal_ !=0;
        if (AssignVal)
        {
            clear();
        }

        string str=string.Empty;
        string messageBuf = string.Empty;

        reader.SkipWhiteSpace();
        ;
        if ( reader.Peek()!=0 ) {
            var c = reader.Read();
            if ((c == '\"') || c.IsXDigit())
            {
                int validDelimiters = 1;
                if (c == '\"')
                {
                    c = reader.Read(); // push past the delimiter
                           // since found a valid delimiter it is now invalid until the
                           //   matching ending delim is found
                    validDelimiters = 0;
                }
                while ( reader.Peek()!=0 && c.IsXDigit() ) {
                    str += c;
                    c = reader.Read();
                }
                if ( reader.Peek()!=0 && (c != '\"') ) {
                    reader.Seek(-1);
                }
                if (AssignVal && (str.Length > 0))
                {
                    content.Clear();
                    content.Append(str);
                }

                if (c == '\"')
                { // if found ending delimiter
                  // if expecting delim (i.e. validDelimiter == 0)
                    if (validDelimiters==0)
                    {
                        validDelimiters = 1; // everything is fine
                    }
                    else
                    { // found ending delimiter but no initial delimiter
                        validDelimiters = 0;
                    }
                }
                // didn't find any delimiters at all and need them.
                else if (needDelims!=0)
                {
                    validDelimiters = 0;
                }

                if (validDelimiters == 0)
                {
                    err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    if (needDelims != 0)
                        messageBuf = "Binary value missing double quote delimiters.\n";
                    else
                        messageBuf = "Mismatched double quote delimiters for binary.\n";
                    err.AppendToDetailMsg(messageBuf);
                    err.AppendToUserMsg(messageBuf);
                }
            }
            else
            {
                err.GreaterSeverity(Severity.SEVERITY_WARNING);
                messageBuf = "Invalid binary value.\n";
                err.AppendToDetailMsg(messageBuf);
                err.AppendToUserMsg(messageBuf);
            }
        } else
        {
            err.GreaterSeverity(Severity.SEVERITY_INCOMPLETE);
        }
        return err.severity();
    }
}