namespace StepCodeDotNet.Base;

using System.Collections.Frozen;
using System.Text;


public interface IStepObjCreator
{
    protected Encoding Encoding { get; }
    protected FrozenSet<string> LeftNames { get; }
    protected FrozenSet<string> RightNames { get; }
    protected FrozenSet<string> ComplexNames { get; }

    public IStepBaseObj Create(ReadOnlySpan<byte> entityName)
    {
        Span<char> charName = stackalloc char[entityName.Length];
        for (int i = 0; i < entityName.Length; i++)
        {
            charName[i] = (char)entityName[i];
        }
        return Create(charName);
    }
    public IStepBaseObj Create(ReadOnlySpan<char> entityName);
    public IStepBaseObj Create(ReadOnlySpan<byte> entityName, ReadOnlySpan<IStepToken> argTokens)
    {
        Span<char> charName = stackalloc char[entityName.Length];
        for (int i = 0; i < entityName.Length; i++)
        {
            charName[i] = (char)entityName[i];
        }
        return Create(charName, argTokens);
    }

    public IStepBaseObj Create(ReadOnlySpan<char> entityName, ReadOnlySpan<IStepToken> argTokens);

    public IStepObj CreateComplex(ReadOnlySpan<IStepToken> complexExpress)
    {
        foreach (var leftToken in complexExpress)
        {
            if (leftToken.TokenType != StepTokenType.Entity)
            {
                continue;
            }
            var leftName = Encoding.UTF8.GetString(leftToken.GetEntityName());
            if (LeftNames.Contains(leftName) is false)
            {
                continue;
            }
            foreach (var rightToken in complexExpress)
            {
                if (rightToken.TokenType != StepTokenType.Entity)
                {
                    continue;
                }
                var rightName = Encoding.UTF8.GetString(rightToken.GetEntityName());
                if (RightNames.Contains(rightName) is false)
                {
                    continue;
                }
                var complexName = $"{leftName}_AND_{rightName}";
                if (ComplexNames.Contains(complexName))
                {
                    return CreateComplex(complexName);
                }
            }
        }
        return default;
    }

    public IStepObj CreateComplex(ReadOnlySpan<char> complexName);

    public void InitStepObj(IStepObj obj, ReadOnlySpan<IStepToken> argTokens, Dictionary<int, IStepObj> refMap);

    public Array CreateArray(Type elementType, int size)
    {
        return Array.CreateInstance(elementType, size);
    }

    public T GetEnum<T>(IStepToken express) where T : struct, Enum
    {
        if (express.TokenType != StepTokenType.Enum)
        {
            return (T)(object)-1;
        }
        var byteValue = express.GetEnumValue();
        if (byteValue.Length == 1 && byteValue[0] == 'U')
        {
            return (T)(object)LOGICAL.UNKNOWN;
        }
        Span<char> charValue = stackalloc char[byteValue.Length];
        for (int i = 0; i < byteValue.Length; i++)
        {
            charValue[i] = (char)byteValue[i];
        }
        return Enum.Parse<T>(charValue);
    }


    public double GetREAL(IStepToken express)
    {
        return express.TokenType switch
        {
            StepTokenType.Real => express.GetRealValue(),
            StepTokenType.Integer => express.GetIntegerValue(),
            _ => 0.0,
        };
    }


    public int GetINTEGER(IStepToken express)
    {
        return express.TokenType switch
        {
            StepTokenType.Integer => express.GetIntegerValue(),
            _ => 0,
        };
    }

    public double GetNUMBER(IStepToken express)
    {
        return express.TokenType switch
        {
            StepTokenType.Real => express.GetRealValue(),
            StepTokenType.Integer => express.GetIntegerValue(),
            _ => 0.0,
        };
    }


    public string GetSTRING(IStepToken express)
    {
        return express.TokenType switch
        {
            StepTokenType.String => Encoding.GetString(express.GetStringValue()),
            _ => string.Empty,
        };
    }

    public byte[] GetBINARY(IStepToken express)
    {
        throw new NotSupportedException("BINARY type is not supported in this context.");
    }


    public bool GetBOOLEAN(IStepToken express) => express.TokenType switch
    {
        StepTokenType.Boolean => express.GetBooleanValue(),
        _ => true,
    };

    public static ReadOnlySpan<IStepToken> GetEntityArgs(ReadOnlySpan<IStepToken> express, out bool hasArgs, out int endIndex)
    {
        hasArgs = false;
        endIndex = express.Length;
        if (express.Length < 2)
        {
            return default;
        }
        if (express[0].TokenType != StepTokenType.LeftBracket)
        {
            return default;
        }
        int bracketLevel = 1;
        endIndex = 1;
        for (int i = 1; i < express.Length; i++)
        {
            var token = express[i];
            if (token.TokenType == StepTokenType.LeftBracket)
            {
                bracketLevel++;
            }
            else if (token.TokenType == StepTokenType.RightBracket)
            {
                bracketLevel--;
                if (bracketLevel == 0)
                {
                    endIndex = i + 1;
                    hasArgs = true;
                    return express[1..endIndex];
                }
            }
            else if (token.TokenType == StepTokenType.Semicolon && bracketLevel == 1)
            {
                endIndex = i + 1;
                hasArgs = true;
                return express[1..endIndex];
            }
        }
        if (bracketLevel > 1)
        {
            return default; // Unmatched brackets
        }
        // If we reach here, it means we have a complete argument list
        endIndex = express.Length;
        hasArgs = true;
        return express[1..endIndex];

    }

    public static ReadOnlySpan<IStepToken> GetListTokens(ReadOnlySpan<IStepToken> express, out bool valid, out int endIndex)
    {
        return GetEntityArgs(express, out valid, out endIndex);
    }


    public (T, int) GetEntity<T>(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap) where T : class
    {
        if (express.Length == 0)
        {
            return default;
        }
        var firstToken = express[0];
        if (firstToken.TokenType == StepTokenType.Entity)
        {
            var remains = express[1..];
            var args = GetEntityArgs(remains, out var hasArgs, out var endIndex);
            IStepBaseObj r = null;
            if (hasArgs)
            {
                r = Create(firstToken.GetEntityName(), args);
            }
            else
            {
                r = Create(firstToken.GetEntityName());
            }
            if (r is IStepObj stepObj)
            {
                if (hasArgs)
                {
                    InitStepObj(stepObj, args, refMap);
                }
            }
            return ((T)r, endIndex);
        }
        else if (firstToken.TokenType == StepTokenType.LineNumber)
        {
            if (refMap.TryGetValue(firstToken.GetLineNumber(), out var stepObj))
            {
                return (stepObj as T, 1);
            }
        }
        return (default, 1);
    }

    public (T, int) GetBaseEntity<T>(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap) where T : unmanaged, IStepBaseObj
    {
        if (express.Length == 0)
        {
            return default;
        }
        var firstToken = express[0];
        if (firstToken.TokenType == StepTokenType.Entity)
        {
            var args = GetEntityArgs(express[1..], out var hasArgs, out var endIndex);
            IStepBaseObj r = null;
            if (hasArgs)
            {
                r = Create(firstToken.GetEntityName(), args);
            }
            else
            {
                r = Create(firstToken.GetEntityName());
            }
            return ((T)r, endIndex);
        }
        return (default, 1);
    }

    private static T[] GetRefAggregate<T>(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap)
    {
        var n = (express.Length + 1) / 2;
        var r = new T[n];
        int i = 0;
        foreach (var token in express)
        {
            if (token.TokenType != StepTokenType.LineNumber)
            {
                continue;
            }
            if (refMap.TryGetValue(token.GetLineNumber(), out var stepObj))
            {
                if (typeof(T).IsAssignableFrom(stepObj.GetType()) is false)
                {
                    Console.WriteLine($"Warning: {stepObj.GetType().Name} is not assignable to {typeof(T).Name},obj line_tag: #{stepObj.line_tag}");
                    continue;
                }
                r[i] = (T)stepObj;
                i++;
            }
        }
        if (i < n)
        {
            Array.Resize(ref r, i);
        }
        return r;
    }

    private Array GetRefAggregateObjs(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap, Type elementType)
    {
        var n = (express.Length + 1) / 2;
        var temp = new IStepObj[n];
        int i = 0;
        foreach (var token in express)
        {
            if (token.TokenType != StepTokenType.LineNumber)
            {
                continue;
            }
            if (refMap.TryGetValue(token.GetLineNumber(), out var stepObj))
            {
                temp[i] = stepObj;
                i++;
            }
        }
        var r = CreateArray(elementType, i);
        Array.Copy(temp, r, i);
        return r;
    }
    private static Type CreateArrayType(Type elementType, int depth)
    {
        var type = elementType;
        for (int i = 0; i < depth; i++)
        {
            type = type.MakeArrayType();
        }
        return type;
    }

    private Array GetList(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap, int arrayDepth, Type elementType)
    {
        var bracketCount = arrayDepth - 1;
        if (bracketCount == 0)
        {
            var firstElement = express[0];
            if (firstElement.TokenType == StepTokenType.LineNumber)
            {
                return GetRefAggregateObjs(express, refMap, elementType);
            }
            else if (firstElement.TokenType == StepTokenType.Entity)
            {
                throw new NotSupportedException("Entity type not supported for list creation.");
            }
            else
            {
                var n = (express.Length + 1) / 2;
                var result = CreateArray(elementType, n);
                int i = 0;
                Span<IStepToken> argTokens = stackalloc IStepToken[1];
                foreach (var token in express)
                {
                    if (token.TokenType != firstElement.TokenType)
                    {
                        continue;
                    }
                    argTokens[0] = token;
                    var typeName = elementType.Name.ToUpper();
                    var r = Create(typeName, argTokens);
                    result.SetValue(r, i);
                    i++;
                }
                return result;
            }
        }
        var subArrays = new List<Array>();
        var subTokensStart = 0;
        int bracketLevel = 0;
        for (int i = 0; i < express.Length; i++)
        {
            var token = express[i];
            if (token.TokenType == StepTokenType.LeftBracket)
            {
                bracketLevel++;
                if (bracketLevel == 1)
                {
                    subTokensStart = i + 1;
                    continue;
                }
            }
            else if (token.TokenType == StepTokenType.RightBracket)
            {
                bracketLevel--;
                if (bracketLevel == 0)
                {
                    var subTokensLength = i - subTokensStart + 1;
                    var subTokens = express.Slice(subTokensStart, subTokensLength);
                    var subArray = GetList(subTokens, refMap, arrayDepth - 1, elementType);
                    subArrays.Add(subArray);
                    continue;
                }
            }
        }

        var arrayType = CreateArrayType(elementType, arrayDepth - 1);
        var mdArray = Array.CreateInstance(arrayType, subArrays.Count);

        for (int i = 0; i < subArrays.Count; i++)
        {
            mdArray.SetValue(subArrays[i], i);
        }
        return mdArray;
    }

    private static (int, Type) GetArrayDepthAndElementType(Type type)
    {
        int depth = 0;
        while (type.IsArray)
        {
            depth++;
            type = type.GetElementType();
        }
        return (depth, type);
    }

    public T[] GetAggregate<T>(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap)
    {

        if (express.Length == 0)
        {
            return [];
        }
        if (typeof(T).IsArray)
        {
            var (depth, elementType) = GetArrayDepthAndElementType(typeof(T[]));
            return (T[])GetList(express, refMap, depth, elementType);
        }
        var firstElement = express[0];
        if (firstElement.TokenType == StepTokenType.LineNumber)
        {
            return GetRefAggregate<T>(express, refMap);
        }
        else if (firstElement.TokenType == StepTokenType.Entity)
        {
            // var args = GetEntityArgs(express[1..], out var hasArgs, out var endIndex);
            // if (hasArgs is false)
            // {
            //     return [];
            // }
            // var argsLength = endIndex - 1;
            // var typeName = typeof(T).Name.ToUpper();
            // var n = (express.Length + 1) / (argsLength + 1);
            // int i = 0;
            // Span<IStepToken> argTokens = stackalloc IStepToken[argsLength];
            // for (int j = 0; i < express.Length; j += argsLength + 1)
            // {
            // }
            throw new NotSupportedException("Entity type not supported for aggregate creation.");
        }
        else
        {
            var typeName = typeof(T).Name.ToUpper();
            var n = (express.Length + 1) / 2;
            var result = new T[n];
            int i = 0;
            Span<IStepToken> argTokens = stackalloc IStepToken[1];
            foreach (var token in express)
            {
                if (token.TokenType != firstElement.TokenType)
                {
                    continue;
                }
                argTokens[0] = token;
                var r = Create(typeName, argTokens);
                result[i] = (T)r;
                i++;
            }
            return result;
        }
    }

}
