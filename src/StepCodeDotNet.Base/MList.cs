using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StepCodeDotNet.Base;

internal class MList<T>(int capacity)
{
    private T[] _data = new T[capacity];
    private int _count = 0;
    private int _capacity = capacity;

    public ref T this[int index]
    {
        get
        {
            Debug.Assert(index < _count);
            return ref _data[index];
        }
    }

    public Span<T> this[Range range]
    {
        get
        {
            return _data[range];
        }
    }


    public int Count => _count;

    public int Capacity => _capacity;

    public Span<T> Data => _data;

    public MList() : this(4)
    {
    }

    public void EnsureCapacity(int min)
    {
        if (_capacity < min)
        {
            _capacity = _capacity == 0 ? 1 : _capacity * 2;
            Array.Resize(ref _data, _capacity);
        }
    }

    public void Add(T item)
    {
        EnsureCapacity(_count + 1);
        _data[_count++] = item;
    }

    public void AddRange(Span<T> items)
    {
        EnsureCapacity(_count + items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            _data[_count++] = items[i];
        }
    }

    public void Insert(int index, T item)
    {
        EnsureCapacity(_count + 1);
        for (int i = _count; i > index; i--)
        {
            _data[i] = _data[i - 1];
        }
        _data[index] = item;
        _count++;
    }

    public void RemoveAt(int index)
    {
        for (int i = index; i < _count - 1; i++)
        {
            _data[i] = _data[i + 1];
        }
        _count--;
    }

    public void RemoveRange(int index, int count)
    {
        for (int i = index; i < _count - count; i++)
        {
            _data[i] = _data[i + count];
        }
        _count -= count;
    }

    public void Remove(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_data[i].Equals(item))
            {
                RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveAll(Predicate<T> match)
    {
        for (int i = 0; i < _count; i++)
        {
            if (match(_data[i]))
            {
                RemoveAt(i);
                i--;
            }
        }
    }

    public ref T At(int index)
    {
        return ref _data[index];
    }

    public ReadOnlySpan<T> AsReadOnlySpan()
    {
        return new ReadOnlySpan<T>(_data, 0, _count);
    }

    public Span<T> AsSpan()
    {
        return new Span<T>(_data, 0, _count);
    }

    public Span<T> Slice(int start, int length)
    {
        return new Span<T>(_data, start, length);
    }

    public T[] ToArray()
    {
        var array = new T[_count];
        Array.Copy(_data, array, _count);
        return array;
    }

    public void Clear()
    {
        _count = 0;
    }


    public static implicit operator Span<T>(MList<T> list)
    {
        return list.AsSpan();
    }

    public static implicit operator ReadOnlySpan<T>(MList<T> list)
    {
        return list.AsReadOnlySpan();
    }

    public static implicit operator T[](MList<T> list)
    {
        return list.ToArray();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_data, _count);
    }

    public ref struct Enumerator(T[] data, int count)
    {
        private readonly T[] _data = data;
        private readonly int _count = count;
        private int index = -1;

        public bool MoveNext()
        {
            index++;
            return index < _count;
        }

        public ref T Current
        {
            get
            {
                return ref _data[index];
            }
        }
    }

}
