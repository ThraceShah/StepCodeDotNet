using System.Runtime.InteropServices;
using System.Text;

namespace StepCodeDotNet;
public static unsafe class StepUtils
{
    public static int strchr(char* str, char c)
    {
        int i = 0;
        while (str[i] != 0)
        {
            if (str[i] == c)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

    public static int strchr(sbyte* str, sbyte c)
    {
        int i = 0;
        while (str[i] != 0)
        {
            if (str[i] == c)
            {
                return i;
            }
            i++;
        }
        return 0;
    }


    public static Severity CheckRemainingInput(this StreamReader reader, ErrorDescriptor err, string typeName, char* tokenList)
    {
        StringBuilder skipBuf = new();
        StringBuilder errMsg = new();

        if (reader.EndOfStream)
        {
            // no error
            return err.severity();
        }
        else if (reader.BaseStream.CanRead == false)
        {
            // Bad bit must have been set during read. Recovery is impossible.
            err.GreaterSeverity(Severity.SEVERITY_INPUT_ERROR);
            errMsg.AppendFormat("Invalid {0} value.\n", typeName);
            err.AppendToUserMsg(errMsg.ToString());
            err.AppendToDetailMsg(errMsg.ToString());
        }
        else
        {
            // At most the fail bit is set, so stream can still be read.
            // Clear errors and skip whitespace.
            reader.SkipWhiteSpace();

            if (reader.EndOfStream)
            {
                // no error
                return err.severity();
            }

            if (tokenList != null)
            {
                // If the next char is a delimiter then there's no error.
                char c = (char)reader.Peek();
                if (strchr(tokenList, c) == 0)
                {
                    // Error. Extra input is more than just a delimiter and is
                    // now considered invalid. We'll try to recover by skipping
                    // to the next delimiter.
                    for (c = (char)reader.Read(); !reader.EndOfStream && strchr(tokenList, c) != 0; c = (char)reader.Read())
                    {
                        skipBuf.Append(c);
                    }

                    if (reader.Peek() != -1 && strchr(tokenList, (char)reader.Peek()) != 0)
                    {
                        // Delimiter found. Recovery succeeded.
                        c = (char)reader.Read();
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);

                        errMsg.AppendFormat("\tFound invalid {0} value...\n", typeName);
                        err.AppendToUserMsg(errMsg.ToString());
                        err.AppendToDetailMsg(errMsg.ToString());
                        err.AppendToDetailMsg("\tdata lost looking for end of attribute: ");
                        err.AppendToDetailMsg(skipBuf.ToString());
                        err.AppendToDetailMsg("\n");

                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    }
                    else
                    {
                        // No delimiter found. Recovery failed.
                        errMsg.AppendFormat("Unable to recover from input error while reading {0} value.\n", typeName);
                        err.AppendToUserMsg(errMsg.ToString());
                        err.AppendToDetailMsg(errMsg.ToString());

                        err.GreaterSeverity(Severity.SEVERITY_INPUT_ERROR);
                    }
                }
            }
            else if (reader.BaseStream.CanRead)
            {
                // Error. Have more input, but lack of delimiter list means we
                // don't know where we can safely resume. Recovery is impossible.
                err.GreaterSeverity(Severity.SEVERITY_WARNING);

                errMsg.AppendFormat("Invalid {0} value.\n", typeName);

                err.AppendToUserMsg(errMsg.ToString());
                err.AppendToDetailMsg(errMsg.ToString());
            }
        }
        return err.severity();
    }

    public static Severity CheckRemainingInput(this SDAIStream<sbyte> reader, ErrorDescriptor err, string typeName, sbyte* tokenList)
    {
        StringBuilder skipBuf = new();
        StringBuilder errMsg = new();

        if (reader.EndOfStream())
        {
            // no error
            return err.severity();
        }
        else if (reader.CanRead() == false)
        {
            // Bad bit must have been set during read. Recovery is impossible.
            err.GreaterSeverity(Severity.SEVERITY_INPUT_ERROR);
            errMsg.AppendFormat("Invalid {0} value.\n", typeName);
            err.AppendToUserMsg(errMsg.ToString());
            err.AppendToDetailMsg(errMsg.ToString());
        }
        else
        {
            // At most the fail bit is set, so stream can still be read.
            // Clear errors and skip whitespace.
            reader.SkipWhiteSpace();

            if (reader.EndOfStream())
            {
                // no error
                return err.severity();
            }

            if (tokenList != null)
            {
                // If the next char is a delimiter then there's no error.
                var c = reader.Peek();
                if (strchr(tokenList, c) == 0)
                {
                    // Error. Extra input is more than just a delimiter and is
                    // now considered invalid. We'll try to recover by skipping
                    // to the next delimiter.
                    for (c = reader.Read(); !reader.EndOfStream() && strchr(tokenList, c) != 0; c = reader.Read())
                    {
                        skipBuf.Append(c);
                    }

                    if (reader.Peek() != -1 && strchr(tokenList, reader.Peek()) != 0)
                    {
                        // Delimiter found. Recovery succeeded.
                        c = reader.Read();
                        reader.Seek(-1);

                        errMsg.AppendFormat("\tFound invalid {0} value...\n", typeName);
                        err.AppendToUserMsg(errMsg.ToString());
                        err.AppendToDetailMsg(errMsg.ToString());
                        err.AppendToDetailMsg("\tdata lost looking for end of attribute: ");
                        err.AppendToDetailMsg(skipBuf.ToString());
                        err.AppendToDetailMsg("\n");

                        err.GreaterSeverity(Severity.SEVERITY_WARNING);
                    }
                    else
                    {
                        // No delimiter found. Recovery failed.
                        errMsg.AppendFormat("Unable to recover from input error while reading {0} value.\n", typeName);
                        err.AppendToUserMsg(errMsg.ToString());
                        err.AppendToDetailMsg(errMsg.ToString());

                        err.GreaterSeverity(Severity.SEVERITY_INPUT_ERROR);
                    }
                }
            }
            else if (reader.CanRead())
            {
                // Error. Have more input, but lack of delimiter list means we
                // don't know where we can safely resume. Recovery is impossible.
                err.GreaterSeverity(Severity.SEVERITY_WARNING);

                errMsg.AppendFormat("Invalid {0} value.\n", typeName);

                err.AppendToUserMsg(errMsg.ToString());
                err.AppendToDetailMsg(errMsg.ToString());
            }
        }
        return err.severity();

    }

    public static Severity CheckRemainingInput(this SDAIStream<sbyte> reader, ErrorDescriptor err, sbyte* typeNameCStr, sbyte* tokenList)
    {
        var typeName = Marshal.PtrToStringAnsi((nint)typeNameCStr);
#pragma warning disable CS8604
        return CheckRemainingInput(reader, err, typeName, tokenList);
#pragma warning restore CS8604
    }
}