namespace StepCodeDotNet.Editor;

using System.Text;
using StepCodeDotNet.Base;

public interface IStepToken
{
}

public record LineNumberToken(int LineNumber) : IStepToken
{
}
public record EqualToken : IStepToken
{
}
public record EntityToken(string EntityName) : IStepToken
{
}
public record LeftBracketToken : IStepToken
{
}
public record RightBracketToken : IStepToken
{
}
public record CommaToken : IStepToken
{
}
public record IntegerToken(int Value) : IStepToken
{
}
public record RealToken(double Value) : IStepToken
{
}
public record StringToken(string Value) : IStepToken
{
}
public record EnumToken(string Value) : IStepToken
{
}
public record SemicolonToken : IStepToken
{
}
public record AsteriskToken : IStepToken
{
}
public record BooleanToken(bool Value) : IStepToken
{
}

public class StepResolver(IStepObjCreater creater)
{
    public List<IStepObj> StepEntities { get; set; } = [];

    public void Resolve(string stepPath)
    {
        var tokens = Tokenize(stepPath);
        PrintTokens(tokens);
    }

    private void PrintTokens(List<IStepToken> tokens)
    {
        foreach (var token in tokens)
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

    private void SkipHeader(StreamReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("DATA;"))
            {
                break;
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
        StringBuilder sb = new();
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsDigit(line[i]) && line[i] != '.')
            {
                endIndex = i - 1;
                break;
            }
            sb.Append(line[i]);
        }
        var str = sb.ToString();
        if (str.Contains('.'))
        {
            return (new RealToken(double.Parse(str)), endIndex);
        }
        else
        {
            return (new IntegerToken(int.Parse(str)), endIndex);
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
                endIndex = i;
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
                    if (char.IsDigit(c))
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

    private List<IStepToken> Tokenize(string stepFile)
    {
        using var reader = new StreamReader(stepFile);
        var tokens = new List<IStepToken>();
        SkipHeader(reader);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("ENDSEC;"))
            {
                break;
            }
            var token = TokenizeLine(line);
            tokens.AddRange(token);
        }
        return tokens;
    }
}
