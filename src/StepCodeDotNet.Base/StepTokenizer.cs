using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace StepCodeDotNet.Base;

public enum StepTokenType
{
    LineNumber,
    Equal,
    Entity,
    LeftBracket,
    RightBracket,
    Comma,
    Integer,
    Real,
    String,
    Enum,
    Semicolon,
    Asterisk,
    Boolean,
    Dollar
}
public unsafe struct IStepToken
{
    private readonly nint _taggedValue;

    private const int TYPE_BITS = 4;
    private const nint TYPE_MASK = (1 << TYPE_BITS) - 1;
    private const int VALUE_BITS = 60; // 64 - 4 = 60位用于存储值或指针

    public readonly StepTokenType TokenType => (StepTokenType)(_taggedValue & TYPE_MASK);

    // 检查是否为直接存储的值
    public readonly bool IsDirectValue => TokenType switch
    {
        StepTokenType.Integer => true,
        StepTokenType.Boolean => true,
        StepTokenType.LineNumber => true,
        StepTokenType.Equal => true,
        StepTokenType.LeftBracket => true,
        StepTokenType.RightBracket => true,
        StepTokenType.Comma => true,
        StepTokenType.Semicolon => true,
        StepTokenType.Asterisk => true,
        StepTokenType.Dollar => true,
        _ => false
    };

    // 对于存储指针的token（复杂类型）
    public readonly ref T As<T>() where T : unmanaged
    {
        Debug.Assert(!IsDirectValue, "Cannot call As<T>() on a direct value token.");
        // 清除类型位，获取指针
        var ptr = (void*)(_taggedValue & ~TYPE_MASK);
        return ref *(T*)ptr;
    }

    // 获取直接存储的int值
    public readonly int GetIntValue()
    {
        Debug.Assert(TokenType == StepTokenType.Integer || TokenType == StepTokenType.LineNumber);
        return (int)(_taggedValue >> TYPE_BITS);
    }

    // 获取直接存储的bool值
    public readonly bool GetBoolValue()
    {
        Debug.Assert(TokenType == StepTokenType.Boolean);
        return (_taggedValue >> TYPE_BITS) != 0;
    }

    // 构造函数：指针存储（复杂类型）
    public IStepToken(void* ptr, StepTokenType type)
    {
        // 确保指针低4位为0（自然对齐）
        Debug.Assert(((nint)ptr & TYPE_MASK) == 0, "Pointer must be aligned to 16 bytes");
        _taggedValue = (nint)ptr | (nint)type;
    }

    // 构造函数：直接值存储（int）
    public IStepToken(int value, StepTokenType type)
    {
        Debug.Assert(type == StepTokenType.Integer || type == StepTokenType.LineNumber);
        _taggedValue = ((nint)value << TYPE_BITS) | (nint)type;
    }

    // 构造函数：直接值存储（bool）
    public IStepToken(bool value, StepTokenType type)
    {
        Debug.Assert(type == StepTokenType.Boolean);
        _taggedValue = (value ? (nint)1 : (nint)0) << TYPE_BITS | (nint)type;
    }

    // 构造函数：无值token（标记类型）
    public IStepToken(StepTokenType type)
    {
        Debug.Assert(type == StepTokenType.Equal || type == StepTokenType.LeftBracket ||
                     type == StepTokenType.RightBracket || type == StepTokenType.Comma ||
                     type == StepTokenType.Semicolon || type == StepTokenType.Asterisk ||
                     type == StepTokenType.Dollar);
        _taggedValue = (nint)type;
    }
}
public readonly struct LineNumberToken
{
    public readonly int LineNumber;
    public LineNumberToken(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}
public readonly struct EqualToken()
{
}
public readonly struct LeftBracketToken()
{
}

public readonly struct RightBracketToken()
{
}

public readonly struct CommaToken()
{
}
public readonly struct IntegerToken
{
    public readonly int Value;
    public IntegerToken(int value)
    {
        Value = value;
    }
}
public readonly struct RealToken
{
    public readonly double Value;
    public RealToken(double value)
    {
        Value = value;
    }
}

public readonly unsafe struct EntityToken
{
    public readonly byte* Ptr;
    public readonly int Length;
    public readonly ReadOnlySpan<byte> EntityName => new(Ptr, Length);

    public EntityToken(byte* ptr, int length)
    {
        Ptr = ptr;
        Length = length;
    }
}

public readonly unsafe struct StringToken
{
    public readonly byte* Ptr;
    public readonly int Length;
    public readonly ReadOnlySpan<byte> Value => new(Ptr, Length);

    public StringToken(byte* ptr, int length)
    {
        Ptr = ptr;
        Length = length;
    }
}
public readonly unsafe struct EnumToken
{
    public readonly byte* Ptr;
    public readonly int Length;
    public readonly ReadOnlySpan<byte> Value => new(Ptr, Length);
    public EnumToken(byte* ptr, int length)
    {
        Ptr = ptr;
        Length = length;
    }
}
public readonly struct SemicolonToken()
{
}
public readonly struct AsteriskToken()
{
}
public readonly struct BooleanToken
{
    public readonly bool Value;
    public BooleanToken(bool value)
    {
        Value = value;
    }
}
public readonly struct DollarToken()
{
}


internal readonly ref struct StepTokenizeResult(UMSpanList<IStepToken> tokens, UMSpanList<int> lines)
{
    public readonly UMSpanList<IStepToken> Tokens = tokens;
    public readonly UMSpanList<int> Lines = lines;
}

public unsafe ref struct StepTokensMemoryPool(Int64 capacity)
{
    private readonly byte* _ptr = (byte*)NativeMemory.AlignedAlloc((nuint)capacity, 16);
    private Int64 _used = 0;

    public T* Rent<T>(T value) where T : unmanaged
    {
        // 确保16字节对齐
        var alignedUsed = (_used + 15) & ~15;
        Debug.Assert(alignedUsed + sizeof(T) <= capacity, "Not enough memory in pool to rent the requested type.");
        var ptr = (T*)(_ptr + alignedUsed);
        Unsafe.Write(ptr, value);
        _used = alignedUsed + sizeof(T);
        return ptr;
    }

    public T* RentBuffer<T>(scoped ReadOnlySpan<T> buffer) where T : unmanaged
    {
        // 确保16字节对齐
        var alignedUsed = (_used + 15) & ~15;
        Debug.Assert(alignedUsed + buffer.Length * sizeof(T) <= capacity, "Not enough memory in pool to rent the requested buffer.");
        var ptr = (T*)(_ptr + alignedUsed);
        buffer.CopyTo(new Span<T>(ptr, buffer.Length));
        _used = alignedUsed + buffer.Length * sizeof(T);
        return ptr;
    }

    public Span<T> RentSpan<T>(int length) where T : unmanaged
    {
        // 确保16字节对齐
        var alignedUsed = (_used + 15) & ~15;
        Debug.Assert(alignedUsed + length * sizeof(T) <= capacity, "Not enough memory in pool to rent the requested span.");
        var ptr = (T*)(_ptr + alignedUsed);
        _used = alignedUsed + length * sizeof(T);
        return new Span<T>(ptr, length);
    }

    // 为复杂类型分配内存的Token
    public IStepToken RentToken<T>(T value, StepTokenType type) where T : unmanaged
    {
        var ptr = Rent(value);
        return new IStepToken(ptr, type);
    }

    public IStepToken RentEnumToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new EnumToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr, StepTokenType.Enum);
    }

    public IStepToken RentStringToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new StringToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr, StepTokenType.String);
    }

    public IStepToken RentEntityToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new EntityToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr, StepTokenType.Entity);
    }

    public readonly Int64 RemainingCapacity => capacity - _used;

    public void Dispose()
    {
        NativeMemory.Free(_ptr);
        _used = 0;
    }
}

public unsafe ref struct StepTokenizer
{
    static readonly byte[] _dataStart = Encoding.ASCII.GetBytes("DATA;");
    readonly int _fileSize;
    readonly string _stepFile;
    StepTokensMemoryPool _memoryPool;
    public StepTokenizer(string stepFile)
    {
        _stepFile = stepFile;
        var fileInfo = new FileInfo(stepFile);
        if (fileInfo.Length > int.MaxValue)
        {
            throw new ArgumentException("The STEP file is too large to process.");
        }
        _fileSize = (int)fileInfo.Length;
        long preAllocatedSize = fileInfo.Length * 8;
        PrintPreallocatedMemoryInfo(preAllocatedSize);
        _memoryPool = new StepTokensMemoryPool(preAllocatedSize); // Allocate double the file size for tokens
    }

    private static void PrintPreallocatedMemoryInfo(long preAllocatedSize)
    {
        const int MB = 1024 * 1024;
        const int KB = 1024;
        if (preAllocatedSize >= MB)
        {
            Console.WriteLine($"Pre-allocated memory pool size: {preAllocatedSize / MB} MB");
        }
        else if (preAllocatedSize >= KB)
        {
            Console.WriteLine($"Pre-allocated memory pool size: {preAllocatedSize / KB} KB");
        }
        else
        {
            Console.WriteLine($"Pre-allocated memory pool size: {preAllocatedSize} B");
        }
    }

    private static void SkipHeader(FileStream reader)
    {
        using UMList<byte> sb = new(1024);
        int buffer = 0;
        while ((buffer = reader.ReadByte()) != -1)
        {
            if (buffer == '\r')
            {
                continue;
            }
            if (buffer == '\n')
            {
                continue;
            }
            sb.Add((byte)buffer);
            if (buffer == ';')
            {
                if (sb.AsReadOnlySpan().StartsWith(_dataStart))
                {
                    break;
                }
                sb.Clear();
            }
        }
    }

    private (IStepToken token, int endIndex) GetLineNumber(ReadOnlySpan<byte> line)
    {
        var start = 0;
        while (line[start].IsDigit() && start < line.Length)
        {
            start++;
        }
        var value = int.Parse(line[..start]);
        return (new IStepToken(value, StepTokenType.LineNumber), start);
    }

    private (IStepToken token, int endIndex) GetEnumToken(ReadOnlySpan<byte> line)
    {
        Span<byte> buffer = stackalloc byte[128];
        UMSpanList<byte> sb = new(buffer);
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '.')
            {
                endIndex = i + 1;
                break;
            }
            sb.Add(line[i]);
        }
        if (sb.Count == 0)
        {
            return (new IStepToken(StepTokenType.Dollar), endIndex);
        }
        if (sb.Count == 1)
        {
            if (sb[0] == 'T')
            {
                return (new IStepToken(true, StepTokenType.Boolean), endIndex);
            }
            else if (sb[0] == 'F')
            {
                return (new IStepToken(false, StepTokenType.Boolean), endIndex);
            }
        }
        return (_memoryPool.RentEnumToken(sb), endIndex);
    }

    private (IStepToken token, int endIndex) GetStringToken(ReadOnlySpan<byte> line)
    {
        using UMList<byte> sb = new(128);
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\'')
            {
                endIndex = i + 1;
                break;
            }
            sb.Add(line[i]);
        }
        return (_memoryPool.RentStringToken(sb), endIndex);
    }

    private (IStepToken token, int endIndex) GetNumberToken(ReadOnlySpan<byte> line)
    {
        int endIndex = 1;
        byte b;
        bool isReal = false;
        while (endIndex < line.Length)
        {
            b = line[endIndex];
            if (b.IsDigit())
            {
                endIndex++;
                continue;
            }
            if (b == '.')
            {
                endIndex++;
                isReal = true;
                continue;
            }
            if (b == 'e' || b == 'E' || b == '-' || b == '+')
            {
                endIndex++;
            }
            else
            {
                break;
            }
        }
        var str = line[..endIndex];
        if (isReal)
        {
            return (_memoryPool.RentToken(new RealToken(double.Parse(str)), StepTokenType.Real), endIndex - 1);
        }
        else
        {
            var intValue = int.Parse(str);
            return (new IStepToken(intValue, StepTokenType.Integer), endIndex - 1);
        }

    }

    private (IStepToken token, int endIndex) GetEntityToken(ReadOnlySpan<byte> line)
    {
        using UMList<byte> sb = new(128);
        var endIndex = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (!line[i].IsLetterOrDigit() && line[i] != '_')
            {
                endIndex = i - 1;
                break;
            }
            sb.Add(line[i]);
        }
        return (_memoryPool.RentEntityToken(sb), endIndex);
    }

    private void TokenizeLine(ReadOnlySpan<byte> line, ref UMSpanList<IStepToken> tokens)
    {
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            switch (c)
            {
                case (byte)' ':
                    break;
                case (byte)'#':
                    {
                        var (token, endIndex) = GetLineNumber(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                case (byte)'=':
                    tokens.Add(new IStepToken(StepTokenType.Equal));
                    break;
                case (byte)'(':
                    tokens.Add(new IStepToken(StepTokenType.LeftBracket));
                    break;
                case (byte)')':
                    tokens.Add(new IStepToken(StepTokenType.RightBracket));
                    break;
                case (byte)',':
                    tokens.Add(new IStepToken(StepTokenType.Comma));
                    break;
                case (byte)';':
                    tokens.Add(new IStepToken(StepTokenType.Semicolon));
                    break;
                case (byte)'*':
                    tokens.Add(new IStepToken(StepTokenType.Asterisk));
                    break;
                case (byte)'$':
                    tokens.Add(new IStepToken(StepTokenType.Dollar));
                    break;
                case (byte)'.':
                    {
                        var (token, endIndex) = GetEnumToken(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                case (byte)'\'':
                    {
                        var (token, endIndex) = GetStringToken(line[(i + 1)..]);
                        tokens.Add(token);
                        i += endIndex;
                        break;
                    }
                default:
                    if (c.IsDigit() || c == '+' || c == '-')
                    {
                        var (token, endIndex) = GetNumberToken(line[i..]);
                        tokens.Add(token);
                        i += endIndex;
                    }
                    else if (c.IsLetter())
                    {
                        var (token, endIndex) = GetEntityToken(line[i..]);
                        tokens.Add(token);
                        i += endIndex;
                    }
                    break;
            }
        }
        return;
    }

    internal StepTokenizeResult TokenizeSync()
    {
        using var reader = new FileStream(_stepFile, FileMode.Open, FileAccess.Read);
        SkipHeader(reader);
        var preTokenCount = _fileSize / 2;
        UMSpanList<IStepToken> tokens = new(_memoryPool.RentSpan<IStepToken>(preTokenCount));
        UMSpanList<int> lines = new(_memoryPool.RentSpan<int>(preTokenCount / 6));
        using UMList<byte> sb = new(2048);
        int buffer = 0;
        while ((buffer = reader.ReadByte()) != -1)
        {
            if (buffer == '\r')
            {
                continue;
            }
            if (buffer == '\n')
            {
                continue;
            }
            sb.Add((byte)buffer);
            if (buffer == ';')
            {
                var line = sb.AsReadOnlySpan();
                if (line.Length == 0 || line[0] != '#')
                {
                    sb.Clear();
                    continue;
                }
                TokenizeLine(sb, ref tokens);
                sb.Clear();
                lines.Add(tokens.Count);
            }
        }
        lines.Add(tokens.Count); // Add the last line if it exists
        var result = new StepTokenizeResult(tokens, lines);
        Console.WriteLine($"remaining capacity in memory pool: {_memoryPool.RemainingCapacity / 1024 / 1024} MB");
        return result;
    }

    public void Dispose()
    {
        _memoryPool.Dispose();
    }

}

// 为IStepToken添加便捷的扩展方法
public static class StepTokenExtensions
{
    public static int GetLineNumber(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.LineNumber);
        return token.GetIntValue();
    }

    public static int GetIntegerValue(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.Integer);
        return token.GetIntValue();
    }

    public static bool GetBooleanValue(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.Boolean);
        return token.GetBoolValue();
    }

    public static double GetRealValue(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.Real);
        return token.As<RealToken>().Value;
    }

    public static ReadOnlySpan<byte> GetStringValue(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.String);
        return token.As<StringToken>().Value;
    }

    public static ReadOnlySpan<byte> GetEntityName(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.Entity);
        return token.As<EntityToken>().EntityName;
    }

    public static ReadOnlySpan<byte> GetEnumValue(this IStepToken token)
    {
        Debug.Assert(token.TokenType == StepTokenType.Enum);
        return token.As<EnumToken>().Value;
    }
}