namespace StepCodeDotNet;

public unsafe struct SDAIStream<T> where T : unmanaged
{
    private int _position = 0;
    private T* _data;

    public int Position => _position;

    public SDAIStream(T* data)
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

public static class SDAIStreamEx
{
    public static bool EndOfStream(this SDAIStream<sbyte> stream)
    {
        return stream.Peek() == 0;
    }

    public static bool CanRead(this SDAIStream<sbyte> stream)
    {
        return stream.Peek() != 0;
    }

    public static void SkipWhiteSpace(this SDAIStream<sbyte> stream)
    {
        while (stream.Peek().IsWhiteSpace())
        {
            stream.Read();
        }
    }
}