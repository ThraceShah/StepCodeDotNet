namespace StepCodeDotNet.Base;

using System.Collections.Frozen;
using System.Text;

public interface IStepObjCreator
{
    public Encoding Encoding { get; }

    public IStepBaseObj Create(ReadOnlySpan<byte> entityName, IStepToken express);
    public IStepBaseObj Create(string entityName, IStepToken express);
    public IStepBaseObj Create(EntityToken express);

    public Array CreateArray(string typeName, int size);

    public IStepObj CreateComplex(string complexName);

    public void InitStepObj(IStepObj obj, Dictionary<int, IStepObj> refMap);

    public T GetEnum<T>(IStepToken express) where T : struct, Enum
    {
        if (express.TokenType != StepTokenType.Enum)
        {
            return (T)(object)-1;
        }
        var byteValue = express.GetEntityName();
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

    public byte[] GetBINARY(IStepToken express)
    {
        throw new NotImplementedException();
    }


    public string GetSTRING(IStepToken express)
    {
        return express.TokenType switch
        {
            StepTokenType.String => Encoding.GetString(express.GetEntityName()),
            _ => string.Empty,
        };
    }


    public bool GetBOOLEAN(IStepToken express) => express.TokenType switch
    {
        StepTokenType.Boolean => express.GetBooleanValue(),
        _ => true,
    };

    public T GetEntity<T>(IStepToken express, Dictionary<int, IStepObj> refMap) where T : class
    {
        if (express.TokenType == StepTokenType.Entity)
        {
            var r = Create(express.GetEntityName(), express);
            if (r is IStepObj stepObj)
            {
                InitStepObj(stepObj, refMap);
            }
            return r as T;
        }
        else if (express.TokenType == StepTokenType.LineNumber)
        {
            if (refMap.TryGetValue(express.GetLineNumber(), out var stepObj))
            {
                return stepObj as T;
            }
        }
        return default;
    }

    public T GetBaseEntity<T>(IStepToken express, Dictionary<int, IStepObj> refMap) where T : unmanaged, IStepBaseObj => express.TokenType switch
    {
        StepTokenType.Entity => (T)Create(express.GetEntityName(), express),
        _ => default,
    };

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

    private Array GetRefAggregateObjs(ReadOnlySpan<IStepToken> express, Dictionary<int, IStepObj> refMap, string elementTypeName)
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
        var r = CreateArray(elementTypeName, i);
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
            var typeName = elementType.Name.ToUpper();
            var firstElement = express[0];
            if (firstElement.TokenType == StepTokenType.LineNumber)
            {
                return GetRefAggregateObjs(express, refMap, typeName);
            }
            var n = (express.Length + 1) / 2;
            var result = CreateArray(typeName, n);
            int i = 0;
            foreach (var token in express)
            {
                if (token.TokenType != firstElement.TokenType)
                {
                    continue;
                }
                var r = Create(typeName, token);
                if (r is IStepObj stepObj)
                {
                    InitStepObj(stepObj, refMap);
                }
                result.SetValue(r, i);
                i++;
            }
            return result;
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
        var firstElement = express[0];
        if (typeof(T).IsArray)
        {
            var (depth, elementType) = GetArrayDepthAndElementType(typeof(T[]));
            return (T[])GetList(express, refMap, depth, elementType);
        }

        if (firstElement.TokenType == StepTokenType.LineNumber)
        {
            return GetRefAggregate<T>(express, refMap);
        }
        var typeName = typeof(T).Name.ToUpper();
        var n = (express.Length + 1) / 2;
        var result = new T[n];
        int i = 0;
        foreach (var token in express)
        {
            if (token.TokenType != firstElement.TokenType)
            {
                continue;
            }
            var r = Create(typeName, token);
            if (r is IStepObj stepObj)
            {
                InitStepObj(stepObj, refMap);
            }
            result[i] = (T)r;
            i++;
        }
        return result;
    }


    public IStepObj CreateComplex(ReadOnlySpan<IStepToken> complexExpress);

}
