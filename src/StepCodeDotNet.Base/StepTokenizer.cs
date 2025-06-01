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
    private readonly void* ptr;
    public readonly StepTokenType TokenType => *(StepTokenType*)ptr;
    public readonly ref T As<T>() where T : unmanaged
    {
        return ref *(T*)ptr;
    }
    public IStepToken(void* ptr)
    {
        this.ptr = ptr;
    }
}
public readonly struct LineNumberToken
{
    public readonly StepTokenType TokenType = StepTokenType.LineNumber;
    public readonly int LineNumber;
    public LineNumberToken(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}
public readonly struct EqualToken()
{
    public readonly StepTokenType TokenType = StepTokenType.Equal;
}
public readonly struct LeftBracketToken()
{
    public readonly StepTokenType TokenType = StepTokenType.LeftBracket;
}

public readonly struct RightBracketToken()
{
    public readonly StepTokenType TokenType = StepTokenType.RightBracket;
}

public readonly struct CommaToken()
{
    public readonly StepTokenType TokenType = StepTokenType.Comma;
}
public readonly struct IntegerToken
{
    public readonly StepTokenType TokenType = StepTokenType.Integer;
    public readonly int Value;
    public IntegerToken(int value)
    {
        Value = value;
    }
}
public readonly struct RealToken
{
    public readonly StepTokenType TokenType = StepTokenType.Real;
    public readonly double Value;
    public RealToken(double value)
    {
        Value = value;
    }
}

public readonly unsafe struct EntityToken
{
    public readonly StepTokenType TokenType = StepTokenType.Entity;
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
    public readonly StepTokenType TokenType = StepTokenType.String;
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
    public readonly StepTokenType TokenType = StepTokenType.Enum;
    public readonly byte* Ptr;
    public readonly int Length;
    public EnumToken(byte* ptr, int length)
    {
        Ptr = ptr;
        Length = length;
    }
}
public readonly struct SemicolonToken()
{
    public readonly StepTokenType TokenType = StepTokenType.Semicolon;
}
public readonly struct AsteriskToken()
{
    public readonly StepTokenType TokenType = StepTokenType.Asterisk;
}
public readonly struct BooleanToken
{
    public readonly StepTokenType TokenType = StepTokenType.Boolean;
    public readonly bool Value;
    public BooleanToken(bool value)
    {
        Value = value;
    }
}
public readonly struct DollarToken()
{
    public readonly StepTokenType TokenType = StepTokenType.Dollar;
}


internal readonly ref struct StepTokenizeResult(UMSpanList<IStepToken> tokens, UMSpanList<int> lines)
{
    public readonly UMSpanList<IStepToken> Tokens = tokens;
    public readonly UMSpanList<int> Lines = lines;
}

public unsafe ref struct StepTokensMemoryPool(Int64 capacity)
{
    private readonly byte* _ptr = (byte*)NativeMemory.Alloc((nuint)capacity);
    private int _used = 0;

    public T* Rent<T>(T value) where T : unmanaged
    {
        Debug.Assert(_used + sizeof(T) <= capacity, "Not enough memory in pool to rent the requested type.");
        var ptr = (T*)(_ptr + _used);
        Unsafe.Write(ptr, value);
        _used += sizeof(T);
        return ptr;
    }


    public T* RentBuffer<T>(scoped ReadOnlySpan<T> buffer) where T : unmanaged
    {
        Debug.Assert(_used + buffer.Length * sizeof(T) <= capacity, "Not enough memory in pool to rent the requested buffer.");
        var ptr = (T*)(_ptr + _used);
        buffer.CopyTo(new Span<T>(ptr, buffer.Length));
        _used += buffer.Length * sizeof(T);
        return ptr;
    }

    public Span<T> RentSpan<T>(int length) where T : unmanaged
    {
        Debug.Assert(_used + length * sizeof(T) <= capacity, "Not enough memory in pool to rent the requested span.");
        var ptr = (T*)(_ptr + _used);
        _used += length * sizeof(T);
        return new Span<T>(ptr, length);
    }

    public IStepToken RentToken<T>(T value) where T : unmanaged
    {
        var ptr = Rent(value);
        return new IStepToken(ptr);
    }

    public IStepToken RentEnumToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new EnumToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr);
    }

    public IStepToken RentStringToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new StringToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr);
    }

    public IStepToken RentEntityToken(scoped ReadOnlySpan<byte> buffer)
    {
        var rentBuffer = RentBuffer(buffer);
        var value = new EntityToken(rentBuffer, buffer.Length);
        var ptr = Rent(value);
        return new IStepToken(ptr);
    }

    public void Dispose()
    {
        NativeMemory.Free(_ptr);
        _used = 0;
    }

}

public unsafe readonly ref struct StepTokenizer
{
    static readonly byte[] _dataStart = Encoding.ASCII.GetBytes("DATA;");
    readonly int _fileSize;
    readonly string _stepFile;
    readonly StepTokensMemoryPool _memoryPool;
    public StepTokenizer(string stepFile)
    {
        _stepFile = stepFile;
        var fileInfo = new FileInfo(stepFile);
        if (fileInfo.Length > int.MaxValue)
        {
            throw new ArgumentException("The STEP file is too large to process.");
        }
        _fileSize = (int)fileInfo.Length;
        _memoryPool = new StepTokensMemoryPool(_fileSize * 6); // Allocate double the file size for tokens
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
        return (_memoryPool.RentToken(new LineNumberToken(value)), start);
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
            return (_memoryPool.RentToken(new DollarToken()), endIndex);
        }
        if (sb.Count == 1)
        {
            if (sb[0] == 'T')
            {
                return (_memoryPool.RentToken(new BooleanToken(true)), endIndex);
            }
            else if (sb[0] == 'F')
            {
                return (_memoryPool.RentToken(new BooleanToken(false)), endIndex);
            }
        }
        return (_memoryPool.RentEnumToken(sb.AsReadOnlySpan()), endIndex);
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
        return (_memoryPool.RentStringToken(sb.AsReadOnlySpan()), endIndex);
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
            return (_memoryPool.RentToken(new RealToken(double.Parse(str))), endIndex - 1);
        }
        else
        {
            return (_memoryPool.RentToken(new IntegerToken(int.Parse(str))), endIndex - 1);
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
        return (_memoryPool.RentEntityToken(sb.AsReadOnlySpan()), endIndex);
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
                    tokens.Add(_memoryPool.RentToken(new EqualToken()));
                    break;
                case (byte)'(':
                    tokens.Add(_memoryPool.RentToken(new LeftBracketToken()));
                    break;
                case (byte)')':
                    tokens.Add(_memoryPool.RentToken(new RightBracketToken()));
                    break;
                case (byte)',':
                    tokens.Add(_memoryPool.RentToken(new CommaToken()));
                    break;
                case (byte)';':
                    tokens.Add(_memoryPool.RentToken(new SemicolonToken()));
                    break;
                case (byte)'*':
                    tokens.Add(_memoryPool.RentToken(new AsteriskToken()));
                    break;
                case (byte)'$':
                    tokens.Add(_memoryPool.RentToken(new DollarToken()));
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
        var stopWatch = Stopwatch.StartNew();
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
                TokenizeLine(line, ref tokens);
                sb.Clear();
                lines.Add(tokens.Count);
            }
        }
        lines.Add(tokens.Count); // Add the last line if it exists
        stopWatch.Stop();
        var result = new StepTokenizeResult(tokens, lines);
        Console.WriteLine($"Synchronous tokenization completed in {stopWatch.ElapsedMilliseconds} ms, total lines: {lines.Count - 1}");
        return result;
    }

    public void Dispose()
    {
        _memoryPool.Dispose();
    }

}