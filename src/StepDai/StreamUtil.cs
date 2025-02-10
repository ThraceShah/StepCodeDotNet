namespace StepCodeDotNet;

public static unsafe class StreamUtil
{
    public static void SkipWhiteSpace(this StreamReader reader)
    {
        while (char.IsWhiteSpace((char)reader.Peek()))
        {
            reader.Read();
        }
    }
}