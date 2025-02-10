namespace StepCodeDotNet;

public unsafe struct StepDaiStream<T> where T : unmanaged
{
    private int _position = 0;
    private T* _data;

    public int Position => _position;

    public StepDaiStream(T* data)
    {
        _data = data;
    }

    public T Read()
    {
        _position++;
        return _data[_position - 1];
    }

    public T Peek()
    {
        return _data[_position];
    }

    public void Seek(int position)
    {
        _position += position;
    }

}

public static class StepDaiStreamEx
{
    public static bool EndOfStream(this StepDaiStream<sbyte> stream)
    {
        return stream.Peek() == 0;
    }

    public static bool CanRead(this StepDaiStream<sbyte> stream)
    {
        return stream.Peek() != 0;
    }

    public static void SkipWhiteSpace(this StepDaiStream<sbyte> stream)
    {
        while (stream.Peek().IsWhiteSpace())
        {
            stream.Read();
        }
    }
}