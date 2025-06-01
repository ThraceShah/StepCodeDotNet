﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StepCodeDotNet.Base;

internal unsafe ref struct UMList<T>(int capacity) where T : unmanaged
{
    private int _count = 0;
    private int _capacity = capacity;
    private T* _data = (T*)NativeMemory.Alloc((uint)(capacity * sizeof(T)));

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
            var (start, length) = range.GetOffsetAndLength(_count);
            return new Span<T>(_data + start, length);
        }
    }


    public int Count => _count;

    public int Capacity => _capacity;

    public T* Data => _data;

    public UMList() : this(4)
    {
    }

    public void EnsureCapacity(int min)
    {
        if (_capacity < min)
        {
            _capacity = _capacity == 0 ? 1 : _capacity * 2;
            _data = (T*)NativeMemory.Realloc(_data, (uint)(_capacity * sizeof(T)));
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
        return new ReadOnlySpan<T>(_data, _count);
    }

    public Span<T> AsSpan()
    {
        return new Span<T>(_data, _count);
    }

    public Span<T> Slice(int start, int length)
    {
        return new Span<T>(_data + start, length);
    }

    public T[] ToArray()
    {
        var array = new T[_count];
        for (int i = 0; i < _count; i++)
        {
            array[i] = _data[i];
        }
        return array;
    }

    public void Clear()
    {
        _count = 0;
    }

    public void Fit()
    {
        if (_count == _capacity)
        {
            return;
        }
        _capacity = _count;
        _data = (T*)NativeMemory.Realloc(_data, (uint)(_capacity * sizeof(T)));
    }

    public static implicit operator Span<T>(UMList<T> list)
    {
        return list.AsSpan();
    }

    public static implicit operator ReadOnlySpan<T>(UMList<T> list)
    {
        return list.AsReadOnlySpan();
    }

    public static implicit operator T*(UMList<T> list)
    {
        return list._data;
    }

    public static implicit operator T[](UMList<T> list)
    {
        return list.ToArray();
    }


    public Enumerator GetEnumerator()
    {
        return new Enumerator(_data, _count);
    }

    public void Dispose()
    {
        NativeMemory.Free(_data);
        _data = null;
        _count = 0;
        _capacity = 0;
    }

    public ref struct Enumerator(T* data, int count)
    {
        private readonly T* _data = data;
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


internal ref struct UMSpanList<T>(Span<T> buffer) where T : unmanaged
{
    private readonly Span<T> _buffer = buffer;
    private int _count = 0;

    public readonly ref T this[int index]
    {
        get => ref _buffer[index];
    }

    public readonly int Count => _count;

    public readonly int Capacity => _buffer.Length;

    public void Add(T item)
    {
        _buffer[_count++] = item;
    }

    public void Clear()
    {
        _count = 0;
    }

    public readonly ReadOnlySpan<T> AsReadOnlySpan()
    {
        return _buffer[.._count];
    }

    public readonly Span<T> AsSpan()
    {
        return _buffer[.._count];
    }

}