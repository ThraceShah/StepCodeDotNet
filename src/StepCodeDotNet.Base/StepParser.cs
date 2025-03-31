namespace StepCodeDotNet.Base;

using System.Text;
using System.Text.RegularExpressions;

public interface IStepToken;
public record LineNumberToken(int LineNumber) : IStepToken;
public record EqualToken : IStepToken;
public record EntityToken(string EntityName) : IStepToken;
public record LeftBracketToken : IStepToken;
public record RightBracketToken : IStepToken;
public record CommaToken : IStepToken;
public record IntegerToken(int Value) : IStepToken;
public record RealToken(double Value) : IStepToken;
public record StringToken(string Value) : IStepToken;
public record EnumToken(string Value) : IStepToken;
public record SemicolonToken : IStepToken;
public record AsteriskToken : IStepToken;
public record BooleanToken(bool Value) : IStepToken;
public record DollarToken : IStepToken;

public interface IExpress;
public record StringExpress(string Value) : IExpress;
public record IntegerExpress(int Value) : IExpress;
public record RealExpress(double Value) : IExpress;
public record BooleanExpress(bool Value) : IExpress;
public record EnumExpress(string Value) : IExpress;
public record EntityExpress(string EntityName, List<IExpress> Args) : IExpress;
public record AsteriskExpress : IExpress;
public record ListExpress(List<IExpress> ExpressList) : IExpress;
public record RefExpress(int RefLineNumber) : IExpress;
public record LineExpress(int LineNumber, IExpress Body) : IExpress;
public record DollarExpress : IExpress;

public partial class StepParser(IStepObjCreator creater)
{

    public IStepObj[] Resolve(string stepPath)
    {
        var tokenLists = Tokenize(stepPath);
#if DEBUG
        PrintTokens(tokenLists);
#endif
        var expressList = new List<LineExpress>();
        var lineTokens = new List<IStepToken>();
        foreach (var tokens in tokenLists)
        {
            lineTokens.AddRange(tokens);
            if (lineTokens[^1] is not SemicolonToken)
            {
                continue;
            }
            var lineExpress = ResolveLine(lineTokens.ToArray());
            expressList.Add(lineExpress);
            lineTokens.Clear();
        }
        return creater.CreateStepObjs(expressList);
    }

    private (ListExpress, int) ResolveList(ReadOnlySpan<IStepToken> listTokens)
    {
        var result = new List<IExpress>();
        for (int i = 0; i < listTokens.Length; i++)
        {
            switch (listTokens[i])
            {
                case IntegerToken integer:
                    result.Add(new IntegerExpress(integer.Value));
                    break;
                case RealToken real:
                    result.Add(new RealExpress(real.Value));
                    break;
                case StringToken str:
                    result.Add(new StringExpress(str.Value));
                    break;
                case EnumToken enumToken:
                    result.Add(new EnumExpress(enumToken.Value));
                    break;
                case BooleanToken boolean:
                    result.Add(new BooleanExpress(boolean.Value));
                    break;
                case AsteriskToken:
                    result.Add(new AsteriskExpress());
                    break;
                case LineNumberToken lineNumber:
                    result.Add(new RefExpress(lineNumber.LineNumber));
                    break;
                case DollarToken:
                    result.Add(new DollarExpress());
                    break;
                case EntityToken:
                    {
                        var (entityExpress, endIndex) = ResolveEntity(listTokens[i..]);
                        result.Add(entityExpress);
                        i += endIndex;
                        break;
                    }
                case LeftBracketToken:
                    {
                        var startIndex = i + 1;
                        var (listExpress, endIndex) = ResolveList(listTokens[startIndex..]);
                        result.Add(listExpress);
                        i += endIndex + 1;
                        break;
                    }
                case RightBracketToken:
                    return (new ListExpress(result), i);
                default:
                    break;
            }
        }
        return (new ListExpress(result), 0);
    }

    private (EntityExpress, int) ResolveEntity(ReadOnlySpan<IStepToken> entityTokens)
    {
        if (entityTokens.Length < 3)
        {
            throw new Exception("Invalid entity");
        }
        if (entityTokens[0] is not EntityToken entity)
        {
            throw new Exception("Invalid entity");
        }
        if (entityTokens[1] is not LeftBracketToken)
        {
            throw new Exception("Invalid entity");
        }
        var entityName = entity.EntityName;
        var args = new List<IExpress>();
        for (int i = 2; i < entityTokens.Length; i++)
        {
            switch (entityTokens[i])
            {
                case IntegerToken integer:
                    args.Add(new IntegerExpress(integer.Value));
                    break;
                case RealToken real:
                    args.Add(new RealExpress(real.Value));
                    break;
                case StringToken str:
                    args.Add(new StringExpress(str.Value));
                    break;
                case EnumToken enumToken:
                    args.Add(new EnumExpress(enumToken.Value));
                    break;
                case BooleanToken boolean:
                    args.Add(new BooleanExpress(boolean.Value));
                    break;
                case AsteriskToken:
                    args.Add(new AsteriskExpress());
                    break;
                case DollarToken:
                    args.Add(new DollarExpress());
                    break;
                case LineNumberToken lineNumber:
                    args.Add(new RefExpress(lineNumber.LineNumber));
                    break;
                case EntityToken:
                    {
                        var (entityExpress, endIndex) = ResolveEntity(entityTokens[i..]);
                        args.Add(entityExpress);
                        i += endIndex;
                        break;
                    }
                case LeftBracketToken:
                    {
                        var startIndex = i + 1;
                        var (listExpress, endIndex) = ResolveList(entityTokens[startIndex..]);
                        args.Add(listExpress);
                        i += endIndex + 1;
                        break;
                    }
                case RightBracketToken:
                    return (new EntityExpress(entityName, args), i);
                default:
                    break;
            }
        }
        return (new EntityExpress(entityName, args), 0);
    }

    private LineExpress ResolveLine(ReadOnlySpan<IStepToken> lineTokens)
    {
        if (lineTokens.Length < 3)
        {
            throw new Exception("Invalid line");
        }
        if (lineTokens[0] is not LineNumberToken lineNumber)
        {
            throw new Exception("Invalid line number");
        }
        var lineNumberValue = lineNumber.LineNumber;
        if (lineTokens[1] is not EqualToken)
        {
            throw new Exception("Invalid entity");
        }
        switch (lineTokens[2])
        {
            case EntityToken entity:
                var (entityExpress, _) = ResolveEntity(lineTokens[2..]);
                return new LineExpress(lineNumberValue, entityExpress);
            case LeftBracketToken:
                var (listExpress, _) = ResolveList(lineTokens[3..]);
                return new LineExpress(lineNumberValue, listExpress);
            default:
                throw new Exception("Invalid entity");
        }
    }

    private void PrintTokens(List<List<IStepToken>> tokens)
    {
        foreach (var lineTokens in tokens)
        {
            foreach (var token in lineTokens)
            {
                switch (token)
                {
                    case LineNumberToken lineNumber:
                        Console.Write($"#{lineNumber.LineNumber}");
                        break;
                    case EqualToken:
                        Console.Write("=");
                        break;
                    case EntityToken entity:
                        Console.Write(entity.EntityName);
                        break;
                    case LeftBracketToken:
                        Console.Write("(");
                        break;
                    case RightBracketToken:
                        Console.Write(")");
                        break;
                    case CommaToken:
                        Console.Write(",");
                        break;
                    case IntegerToken integer:
                        Console.Write(integer.Value);
                        break;
                    case RealToken real:
                        Console.Write(real.Value);
                        break;
                    case StringToken str:
                        Console.Write($"'{str.Value}'");
                        break;
                    case EnumToken enumToken:
                        Console.Write($".{enumToken.Value}.");
                        break;
                    case SemicolonToken:
                        Console.WriteLine(";");
                        break;
                    case AsteriskToken:
                        Console.Write("*");
                        break;
                    case DollarToken:
                        Console.Write("$");
                        break;
                    case BooleanToken boolean:
                        if (boolean.Value)
                        {
                            Console.Write(".T.");
                        }
                        else
                        {
                            Console.Write(".F.");
                        }
                        break;
                }
            }
        }
    }

    private void SkipHeader(BinaryReader reader)
    {
        StringBuilder sb = new();
        Span<char> buffer = stackalloc char[1];
        while (reader.Read(buffer) > 0)
        {
            if (buffer[0] == '\r')
            {
                continue;
            }
            if (buffer[0] == '\n')
            {
                continue;
            }
            sb.Append(buffer[0]);
            if (buffer[0] == ';')
            {
                var line = sb.ToString();
                sb.Clear();
                if (line.StartsWith("DATA;"))
                {
                    break;
                }
            }
        }
    }

    private (LineNumberToken token, int endIndex) GetLineNumber(ReadOnlySpan<char> line)
    {
        var start = 0;
        while (char.IsDigit(line[start]) && start < line.Length)
        {
            start++;
        }
        var value = int.Parse(line[..start]);
        return (new(value), start);
    }

    private (IStepToken token, int endIndex) GetEnumToken(ReadOnlySpan<char> line)
    {
        StringBuilder sb = new();
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '.')
            {
                endIndex = i + 1;
                break;
            }
            sb.Append(line[i]);
        }
        var str = sb.ToString();
        switch (str)
        {
            case "T":
                return (new BooleanToken(true), endIndex);
            case "F":
                return (new BooleanToken(false), endIndex);
            default:
                return (new EnumToken(str), endIndex);
        }
    }

    private (IStepToken token, int endIndex) GetStringToken(ReadOnlySpan<char> line)
    {
        StringBuilder sb = new();
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\'')
            {
                endIndex = i + 1;
                break;
            }
            sb.Append(line[i]);
        }
        return (new StringToken(sb.ToString()), endIndex);
    }

    private (IStepToken token, int endIndex) GetNumberToken(ReadOnlySpan<char> line)
    {
        var number = NumberRegex().Match(line.ToString());
        var str = number.Value;
        if (str.Contains('.'))
        {
            return (new RealToken(double.Parse(str)), str.Length - 1);
        }
        else
        {
            return (new IntegerToken(int.Parse(str)), str.Length - 1);
        }
    }

    private (EntityToken token, int endIndex) GetEntityToken(ReadOnlySpan<char> line)
    {
        StringBuilder sb = new();
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsLetterOrDigit(line[i]) && line[i] != '_')
            {
                endIndex = i - 1;
                break;
            }
            sb.Append(line[i]);
        }
        return (new EntityToken(sb.ToString()), endIndex);
    }

    private List<IStepToken> TokenizeLine(ReadOnlySpan<char> line)
    {
        var tokens = new List<IStepToken>();
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            switch (c)
            {
                case ' ':
                    break;
                case '#':
                    {
                        var (token, endIndex) = GetLineNumber(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                case '=':
                    tokens.Add(new EqualToken());
                    break;
                case '(':
                    tokens.Add(new LeftBracketToken());
                    break;
                case ')':
                    tokens.Add(new RightBracketToken());
                    break;
                case ',':
                    tokens.Add(new CommaToken());
                    break;
                case ';':
                    tokens.Add(new SemicolonToken());
                    break;
                case '*':
                    tokens.Add(new AsteriskToken());
                    break;
                case '$':
                    tokens.Add(new DollarToken());
                    break;
                case '.':
                    {
                        var (token, endIndex) = GetEnumToken(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                case '\'':
                    {
                        var (token, endIndex) = GetStringToken(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                default:
                    if (char.IsDigit(c) || c == '+' || c == '-')
                    {
                        var (token, endIndex) = GetNumberToken(line[i..]);
                        tokens.Add(token);
                        i += endIndex;
                    }
                    else if (char.IsLetter(c))
                    {
                        var (token, endIndex) = GetEntityToken(line[i..]);
                        tokens.Add(token);
                        i += endIndex;
                    }
                    break;
            }
        }
        return tokens;
    }

    private List<List<IStepToken>> Tokenize(string stepFile)
    {
        using var fileStream = new FileStream(stepFile, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream, Encoding.UTF8);
        SkipHeader(reader);
        var tokens = new List<List<IStepToken>>();
        StringBuilder sb = new();
        Span<char> buffer = stackalloc char[1];
        while (reader.Read(buffer) > 0)
        {
            if (buffer[0] == '\r')
            {
                continue;
            }
            if (buffer[0] == '\n')
            {
                continue;
            }
            sb.Append(buffer[0]);
            if (buffer[0] == ';')
            {
                var line = sb.ToString();
                sb.Clear();
                if (line.StartsWith("ENDSEC;"))
                {
                    break;
                }
                var lineTokens = TokenizeLine(line);
                tokens.Add(lineTokens);
            }
        }
        // while ((line = reader.ReadLine()) != null)
        // {
        //     if (line.StartsWith("ENDSEC;"))
        //     {
        //         break;
        //     }
        //     var lineTokens = TokenizeLine(line);
        //     tokens.Add(lineTokens);
        // }
        return tokens;
    }

    [GeneratedRegex(@"^[-+]?[0-9]+(\.[0-9]*)?([eE][-+]?[0-9]+)?", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
