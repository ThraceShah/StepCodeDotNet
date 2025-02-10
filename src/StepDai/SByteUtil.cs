namespace StepCodeDotNet;
public unsafe static class SByteUtil
{

    public static bool IsAlpha(this sbyte c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    public static bool IsDigit(this sbyte c)
    {
        return c >= '0' && c <= '9';
    }

    public static bool IsXDigit(this sbyte c)
    {
        return IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
    }

    public static bool IsAlNum(this sbyte c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    public static bool IsStrEnd(this sbyte c)
    {
        return c == 0;
    }

    public static int StrCmp(sbyte* s1, sbyte* s2)
    {
        int i = 0;
        while (s1[i] != 0 && s2[i] != 0)
        {
            if (s1[i] != s2[i])
            {
                return s1[i] - s2[i];
            }
            i++;
        }
        return s1[i] - s2[i];
    }

    public static bool IsWhiteSpace(this sbyte c)
    {
        return char.IsWhiteSpace((char)c);
    }

    public static int StrChr(sbyte* str, sbyte c)
    {
        int i = 0;
        while (str[i] != 0)
        {
            if (str[i] == c)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

}