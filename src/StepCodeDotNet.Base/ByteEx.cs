using System.Runtime.CompilerServices;

namespace StepCodeDotNet.Base;

public static class ByteEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter(this byte b)
    {
        return (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(this byte b)
    {
        return b >= '0' && b <= '9';
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterOrDigit(this byte b)
    {
        return b.IsLetter() || b.IsDigit();
    }
}
