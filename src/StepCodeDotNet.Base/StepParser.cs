namespace StepCodeDotNet.Base;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;

public unsafe partial class StepParser(IStepObjCreator creator)
{
    private static readonly Encoding _gb18030;
    static StepParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _gb18030 = Encoding.GetEncoding("GB18030");
    }

    private List<IStepObj> _stepObjs = [];
    Dictionary<int, IStepObj> _refMap = [];

    public IStepObj[] GetStepObjs() => [.. _stepObjs];

    public bool TryGetStepObj(int lineNumber, out IStepObj stepObj)
    {
        return _refMap.TryGetValue(lineNumber, out stepObj);
    }

    public IStepObj GetStepObj(int lineNumber)
    {
        return _refMap[lineNumber];
    }

    public static long GetMemoryUsedMB()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024 * 1024);
    }

    public void Resolve(string stepPath)
    {
        var stopWatch = Stopwatch.StartNew();
        using var tokenizer = new StepTokenizer(stepPath);
        using var tokenizeResult = tokenizer.TokenizeSync();
        stopWatch.Stop();
        Console.WriteLine($"Tokenization took: {stopWatch.ElapsedMilliseconds} ms");
#if DEBUG
        // PrintTokens(tokenLists);
#endif
        CreateStepObjs(tokenizeResult);
        InitStepObjs(tokenizeResult);
        Console.WriteLine($"峰值内存使用: {GetMemoryUsedMB()} MB");
        return;
    }

    private void CreateStepObjs(StepTokenizeResult tokenizeResult)
    {
        var stopWatch = Stopwatch.StartNew();
        this._stepObjs = new(tokenizeResult.Lines.Count);
        this._refMap = new(tokenizeResult.Lines.Count);
        foreach (var line in tokenizeResult)
        {
            if (line.Length < 3)
            {
                continue; // Skip empty lines or lines with insufficient tokens
            }
            var lineNumber = line[0].GetLineNumber();
            var lineBody = line[2..line.Length];
            var thirdToken = line[2];
            IStepBaseObj obj = null;
            switch (thirdToken.TokenType)
            {
                case StepTokenType.Entity:
                    obj = creator.Create(thirdToken.GetEntityName());
                    break;
                case StepTokenType.LeftBracket:
                    obj = creator.CreateComplex(lineBody);
                    break;
                default:
                    break;
            }
            if (obj is IStepObj stepObj)
            {
                stepObj.line_id = lineNumber;
                _stepObjs.Add(stepObj);
                _refMap[lineNumber] = stepObj;
            }
        }
        stopWatch.Stop();
        Console.WriteLine($"Create objs took: {stopWatch.ElapsedMilliseconds} ms");
    }

    private void InitStepObjs(StepTokenizeResult tokenizeResult)
    {
        var stopWatch = Stopwatch.StartNew();
        foreach (var line in tokenizeResult)
        {
            if (line.Length < 3)
            {
                continue; // Skip empty lines or lines with insufficient tokens
            }
            var lineNumber = line[0].GetLineNumber();
            if (_refMap.TryGetValue(lineNumber, out var stepObj) is false)
            {
                continue;
            }
            var thirdToken = line[2];
            if (thirdToken.TokenType == StepTokenType.Entity)
            {
                var args = IStepObjCreator.GetEntityArgs(line[3..], out var hasArgs, out _);
                if (hasArgs is false)
                {
                    continue; // No arguments to initialize
                }
                creator.InitStepObj(stepObj, args, _refMap);
            }
            else
            {
                creator.InitStepObj(stepObj, line[2..], _refMap);
            }

        }
        stopWatch.Stop();
        Console.WriteLine($"Initialization took: {stopWatch.ElapsedMilliseconds} ms");
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


}
