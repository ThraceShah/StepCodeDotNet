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

    public IStepObj[] Resolve(string stepPath)
    {
        using var tokenizer = new StepTokenizer(stepPath);
        var tokenLists = tokenizer.TokenizeSync();

        //        var stopWatch = Stopwatch.StartNew();
        //#if DEBUG
        //        // PrintTokens(tokenLists);
        //#endif
        //        stopWatch.Restart();
        //        var expressList = new List<LineExpress>();
        //        foreach (var lineTokens in tokenLists)
        //        {
        //            var lineExpress = ResolveLine(lineTokens);
        //            expressList.Add(lineExpress);
        //        }

        //        stopWatch.Stop();
        //        Console.WriteLine($"Expression resolution took: {stopWatch.ElapsedMilliseconds} ms");

        //        stopWatch.Restart();
        //        var stepObjs = creater.CreateStepObjs(expressList);
        //        stopWatch.Stop();
        //        Console.WriteLine($"Object creation took: {stopWatch.ElapsedMilliseconds} ms");
        //        return stepObjs;

        return [];
    }
    /*
    private static (ListExpress, int) ResolveList(ReadOnlySpan<IStepToken> listTokens)
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

    private static ComplexExpress ResolveComplex(ReadOnlySpan<IStepToken> listTokens)
    {
        var result = new List<EntityExpress>();
        for (int i = 0; i < listTokens.Length; i++)
        {
            switch (listTokens[i])
            {
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
                        result.AddRange(listExpress.ExpressList.Select(x => (EntityExpress)x));
                        i += endIndex + 1;
                        break;
                    }
                case RightBracketToken:
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

    private static LineExpress ResolveLine(ReadOnlySpan<IStepToken> lineTokens)
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
            case EntityToken:
                var (entityExpress, _) = ResolveEntity(lineTokens[2..]);
                return new LineExpress(lineNumberValue, entityExpress);
            case LeftBracketToken:
                var complexExpress = ResolveComplex(lineTokens[3..]);
                return new LineExpress(lineNumberValue, complexExpress);
            default:
                throw new Exception("Invalid entity");
        }
    }

    private static void PrintTokens(List<MList<IStepToken>> tokens)
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
    */



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
