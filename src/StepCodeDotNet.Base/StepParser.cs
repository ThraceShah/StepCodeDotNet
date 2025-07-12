namespace StepCodeDotNet.Base;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;


public interface IExpress;
public interface IExpress<T> : IExpress
{
    T Value { get; }
}
public record StringExpress(string Value) : IExpress<string>;
public record struct IntegerExpress(int Value) : IExpress<int>;
public record struct RealExpress(double Value) : IExpress<double>;
public record struct BooleanExpress(bool Value) : IExpress<bool>;
public record EnumExpress(string Value) : IExpress<string>;
public record EntityExpress(string EntityName, List<IExpress> Args) : IExpress;
public record struct AsteriskExpress : IExpress;
public record ListExpress(List<IExpress> ExpressList) : IExpress;
public record ComplexExpress(List<EntityExpress> ExpressList) : IExpress;
public record struct RefExpress(int RefLineNumber) : IExpress;
public record LineExpress(int LineNumber, IExpress Body) : IExpress;
public record struct DollarExpress : IExpress;

public unsafe partial class StepParser(IStepObjCreator creater)
{
    private static readonly Encoding _gb18030;
    static StepParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _gb18030 = Encoding.GetEncoding("GB18030");
    }
    public static long GetMemoryUsedMB()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024 * 1024);
    }

    public IStepObj[] Resolve(string stepPath)
    {
        var totalWatch = Stopwatch.StartNew();
        var lineExps = ResolveToExpress(stepPath);
        Console.WriteLine($"After resolve express, Memory used: {GetMemoryUsedMB()} MB");
        var stopWatch = Stopwatch.StartNew();
        var stepObjs = creater.CreateStepObjs(lineExps);
        stopWatch.Stop();
        totalWatch.Stop();
        Console.WriteLine($"Object creation took: {stopWatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total parsing took: {totalWatch.ElapsedMilliseconds} ms");
        return stepObjs;
    }

    private List<LineExpress> ResolveToExpress(string stepPath)
    {
        var stopWatch = Stopwatch.StartNew();
        using var tokenizer = new StepTokenizer(stepPath);
        var tokenizeResult = tokenizer.TokenizeSync();
        stopWatch.Stop();
        Console.WriteLine($"Tokenization took: {stopWatch.ElapsedMilliseconds} ms");
#if DEBUG
        // PrintTokens(tokenLists);
#endif
        stopWatch.Restart();
        var expressList = new List<LineExpress>();
        foreach (var line in tokenizeResult)
        {
            var lineExpress = ResolveLine(line);
            expressList.Add(lineExpress);
        }
        stopWatch.Stop();
        Console.WriteLine($"Expression resolution took: {stopWatch.ElapsedMilliseconds} ms");
        return expressList;
    }

    private static (ListExpress, int) ResolveList(ReadOnlySpan<IStepToken> listTokens)
    {
        var result = new List<IExpress>();
        for (int i = 0; i < listTokens.Length; i++)
        {
            var token = listTokens[i];
            switch (token.TokenType)
            {
                case StepTokenType.Integer:
                    result.Add(new IntegerExpress(token.GetIntegerValue()));
                    break;
                case StepTokenType.Real:
                    result.Add(new RealExpress(token.GetRealValue()));
                    break;
                case StepTokenType.String:
                    result.Add(new StringExpress(_gb18030.GetString(token.GetStringValue())));
                    break;
                case StepTokenType.Enum:
                    result.Add(new EnumExpress(Encoding.ASCII.GetString(token.GetEnumValue())));
                    break;
                case StepTokenType.Boolean:
                    result.Add(new BooleanExpress(token.GetBooleanValue()));
                    break;
                case StepTokenType.Asterisk:
                    result.Add(new AsteriskExpress());
                    break;
                case StepTokenType.LineNumber:
                    result.Add(new RefExpress(token.GetLineNumber()));
                    break;
                case StepTokenType.Dollar:
                    result.Add(new DollarExpress());
                    break;
                case StepTokenType.Entity:
                    {
                        var (entityExpress, endIndex) = ResolveEntity(listTokens[i..]);
                        result.Add(entityExpress);
                        i += endIndex;
                        break;
                    }
                case StepTokenType.LeftBracket:
                    {
                        var startIndex = i + 1;
                        var (listExpress, endIndex) = ResolveList(listTokens[startIndex..]);
                        result.Add(listExpress);
                        i += endIndex + 1;
                        break;
                    }
                case StepTokenType.RightBracket:
                    return (new ListExpress(result), i);
                default:
                    break;
            }
        }
        return (new ListExpress(result), 0);
    }

    private static ComplexExpress ResolveComplex(ReadOnlySpan<IStepToken> listTokens)
    {
        var result = new List<EntityExpress>();
        for (int i = 0; i < listTokens.Length; i++)
        {
            var token = listTokens[i];
            switch (token.TokenType)
            {
                case StepTokenType.Entity:
                    {
                        var (entityExpress, endIndex) = ResolveEntity(listTokens[i..]);
                        result.Add(entityExpress);
                        i += endIndex;
                        break;
                    }
                case StepTokenType.LeftBracket:
                    {
                        var startIndex = i + 1;
                        var (listExpress, endIndex) = ResolveList(listTokens[startIndex..]);
                        result.AddRange(listExpress.ExpressList.Select(x => (EntityExpress)x));
                        i += endIndex + 1;
                        break;
                    }
                case StepTokenType.RightBracket:
                    return new ComplexExpress(result);
                default:
                    break;
            }
        }
        return new ComplexExpress(result);
    }


    private static (EntityExpress, int) ResolveEntity(ReadOnlySpan<IStepToken> entityTokens)
    {
        if (entityTokens.Length < 3)
        {
            throw new Exception("Invalid entity");
        }
        if (entityTokens[0].TokenType != StepTokenType.Entity)
        {
            throw new Exception("Invalid entity");
        }
        if (entityTokens[1].TokenType != StepTokenType.LeftBracket)
        {
            throw new Exception("Invalid entity");
        }
        var entityName = Encoding.ASCII.GetString(entityTokens[0].GetEntityName());
        var args = new List<IExpress>();
        for (int i = 2; i < entityTokens.Length; i++)
        {
            var token = entityTokens[i];
            switch (token.TokenType)
            {
                case StepTokenType.Integer:
                    args.Add(new IntegerExpress(token.GetIntegerValue()));
                    break;
                case StepTokenType.Real:
                    args.Add(new RealExpress(token.GetRealValue()));
                    break;
                case StepTokenType.String:
                    args.Add(new StringExpress(_gb18030.GetString(token.GetStringValue())));
                    break;
                case StepTokenType.Enum:
                    args.Add(new EnumExpress(Encoding.ASCII.GetString(token.GetEnumValue())));
                    break;
                case StepTokenType.Boolean:
                    args.Add(new BooleanExpress(token.GetBooleanValue()));
                    break;
                case StepTokenType.Asterisk:
                    args.Add(new AsteriskExpress());
                    break;
                case StepTokenType.Dollar:
                    args.Add(new DollarExpress());
                    break;
                case StepTokenType.LineNumber:
                    args.Add(new RefExpress(token.GetLineNumber()));
                    break;
                case StepTokenType.Entity:
                    {
                        var (entityExpress, endIndex) = ResolveEntity(entityTokens[i..]);
                        args.Add(entityExpress);
                        i += endIndex;
                        break;
                    }
                case StepTokenType.LeftBracket:
                    {
                        var startIndex = i + 1;
                        var (listExpress, endIndex) = ResolveList(entityTokens[startIndex..]);
                        args.Add(listExpress);
                        i += endIndex + 1;
                        break;
                    }
                case StepTokenType.RightBracket:
                    return (new EntityExpress(entityName, args), i);
                default:
                    break;
            }
        }
        return (new EntityExpress(entityName, args), 0);
    }

    private static LineExpress ResolveLine(ReadOnlySpan<IStepToken> lineTokens)
    {
        if (lineTokens.Length < 3)
        {
            throw new Exception("Invalid line");
        }
        if (lineTokens[0].TokenType != StepTokenType.LineNumber)
        {
            throw new Exception("Invalid line number");
        }
        var lineNumberValue = lineTokens[0].GetLineNumber();
        if (lineTokens[1].TokenType != StepTokenType.Equal)
        {
            throw new Exception("Invalid equal sign");
        }
        switch (lineTokens[2].TokenType)
        {
            case StepTokenType.Entity:
                var (entityExpress, _) = ResolveEntity(lineTokens[2..]);
                return new LineExpress(lineNumberValue, entityExpress);
            case StepTokenType.LeftBracket:
                var complexExpress = ResolveComplex(lineTokens[3..]);
                return new LineExpress(lineNumberValue, complexExpress);
            default:
                throw new Exception("Invalid entity");
        }
    }

    private static void PrintTokens(StepTokenizeResult tokens)
    {
        int lineStart = 0;
        foreach (var lineEnd in tokens.Lines.AsSpan())
        {
            foreach (var token in tokens.Tokens[lineStart..lineEnd])
            {
                switch (token.TokenType)
                {
                    case StepTokenType.LineNumber:
                        Console.Write($"#{token.GetLineNumber()}");
                        break;
                    case StepTokenType.Equal:
                        Console.Write("=");
                        break;
                    case StepTokenType.Entity:
                        Console.Write(Encoding.ASCII.GetString(token.GetEntityName()));
                        break;
                    case StepTokenType.LeftBracket:
                        Console.Write("(");
                        break;
                    case StepTokenType.RightBracket:
                        Console.Write(")");
                        break;
                    case StepTokenType.Comma:
                        Console.Write(",");
                        break;
                    case StepTokenType.Integer:
                        Console.Write(token.GetIntegerValue());
                        break;
                    case StepTokenType.Real:
                        Console.Write(token.GetRealValue());
                        break;
                    case StepTokenType.String:
                        Console.Write($"'{_gb18030.GetString(token.GetStringValue())}'");
                        break;
                    case StepTokenType.Enum:
                        Console.Write($".{Encoding.ASCII.GetString(token.GetEnumValue())}.");
                        break;
                    case StepTokenType.Semicolon:
                        Console.WriteLine(";");
                        break;
                    case StepTokenType.Asterisk:
                        Console.Write("*");
                        break;
                    case StepTokenType.Dollar:
                        Console.Write("$");
                        break;
                    case StepTokenType.Boolean:
                        if (token.GetBooleanValue())
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
            lineStart = lineEnd;
        }
    }




    // private static ConcurrentBag<MList<IStepToken>> Tokenize(string stepFile)
    // {
    //     using var fileStream = new FileStream(stepFile, FileMode.Open, FileAccess.Read);
    //     using var reader = new BinaryReader(fileStream);
    //     SkipHeader(fileStream);


    //     var tokens = new ConcurrentBag<MList<IStepToken>>();
    //     var channel = Channel.CreateUnbounded<string[]>();
    //     const int batchSize = 100;

    //     var producer = Task.Run(() =>
    //     {
    //         var producerWatch = Stopwatch.StartNew();
    //         var batch = new string[batchSize];
    //         int batchIndex = 0;
    //         using UMList<byte> sb = new(2048);
    //         Span<byte> buffer = stackalloc byte[1];
    //         while (reader.Read(buffer) > 0)
    //         {
    //             if (buffer[0] == '\r')
    //             {
    //                 continue;
    //             }
    //             if (buffer[0] == '\n')
    //             {
    //                 continue;
    //             }
    //             sb.Add(buffer[0]);
    //             if (buffer[0] == ';')
    //             {
    //                 var line = _gb18030.GetString(sb.AsReadOnlySpan());
    //                 sb.Clear();
    //                 if (line.StartsWith("ENDSEC;"))
    //                 {
    //                     break;
    //                 }
    //                 batch[batchIndex++] = line;
    //                 if (batchIndex == batchSize)
    //                 {
    //                     channel.Writer.WriteAsync(batch);
    //                     batch = new string[batchSize];
    //                     batchIndex = 0;
    //                 }
    //             }
    //         }
    //         if (batchIndex > 0)
    //         {
    //             Array.Resize(ref batch, batchIndex);
    //             channel.Writer.TryWrite(batch);
    //         }
    //         channel.Writer.Complete();
    //         producerWatch.Stop();
    //         Console.WriteLine($"Producer completed in {producerWatch.ElapsedMilliseconds} ms");
    //     });
    //     var consumerWatch = Stopwatch.StartNew();
    //     int workerCount = Environment.ProcessorCount;
    //     var consumers = Enumerable.Range(0, workerCount).Select(_ => Task.Run(async () =>
    //     {
    //         await foreach (var batch in channel.Reader.ReadAllAsync())
    //         {
    //             foreach (var line in batch)
    //             {
    //                 var lineTokens = TokenizeLine(line);
    //                 tokens.Add(lineTokens);
    //             }
    //         }
    //     })).ToArray();
    //     Task.WaitAll(consumers);
    //     consumerWatch.Stop();
    //     Console.WriteLine($"Consumer completed in {consumerWatch.ElapsedMilliseconds} ms");
    //     return tokens;
    // }

    [GeneratedRegex(@"^[-+]?[0-9]+(\.[0-9]*)?([eE][-+]?[0-9]+)?", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
