using System;
using System.Runtime.InteropServices;
using StepCodeDotNet;

namespace StepDai
{
    public unsafe class SDAI_PID : SDAI_sdaiObject
    {
        public string _datastore_type = string.Empty;
        public string _pidstring = string.Empty;

        public string Datastore_type()
        {
            return _datastore_type;
        }

        public void Datastore_type(string x)
        {
            _datastore_type = x;
        }

        public string get_PIDString()
        {
            return _pidstring;
        }
    }

    public unsafe class SDAI_PID_DA : SDAI_PID
    {
        public string _oid = string.Empty;

        public SDAI_PID_DA()
        {
        }

        public virtual string oid()
        {
            return _oid;
        }

        public virtual void oid(string x)
        {
            _oid = x;
        }
    }

    public unsafe class SDAI_PID_SDAI : SDAI_PID
    {
        public string _modelid = string.Empty;

        public virtual string Modelid()
        {
            return _modelid;
        }

        public virtual void Modelid(string x)
        {
            _modelid = x;
        }
    }

    public unsafe class SDAI_DAObject : SDAI_sdaiObject
    {
        public string _dado_oid = string.Empty;

        public bool dado_same(SDAI_DAObject obj)
        {
            return obj == this;
        }

        public string dado_oid()
        {
            return _dado_oid;
        }

        public SDAI_PID_DA dado_pid()
        {
            return null;
        }

        public void dado_remove()
        {
        }

        public void dado_free()
        {
        }
    }

    public unsafe class SDAI_DAObject_SDAI : SDAI_DAObject
    {
        public SDAI_DAObject_SDAI()
        {
        }

        public override void Dispose()
        {
        }

        public bool IsSame(SDAI_DAObject otherEntity)
        {
            return otherEntity == this;
        }

        public string GetInstanceTypeName()
        {
            return GetType().Name;
        }
    }

    public unsafe class SDAI_DAObject__set : IDisposable
    {
        private SDAI_DAObject[] _buf;
        private int _bufsize;
        private int _count;

        public SDAI_DAObject__set(int defaultSize = 16)
        {
            _bufsize = defaultSize;
            _buf = new SDAI_DAObject[_bufsize];
            _count = 0;
        }

        public void Dispose()
        {
            _buf = null;
        }

        private void Check(int index)
        {
            if (index >= _bufsize)
            {
                _bufsize = (index + 1) * 2;
                Array.Resize(ref _buf, _bufsize);
            }
        }

        public void Insert(SDAI_DAObject v, int index)
        {
            index = (index < 0) ? _count : index;

            if (index < _count)
            {
                Check(_count + 1);
                Array.Copy(_buf, index, _buf, index + 1, _count - index);
            }
            else
            {
                Check(index);
            }
            _buf[index] = v;
            ++_count;
        }

        public void Append(SDAI_DAObject v)
        {
            Insert(v, _count);
        }

        public void Remove(int index)
        {
            if (0 <= index && index < _count)
            {
                --_count;
                Array.Copy(_buf, index + 1, _buf, index, _count - index);
            }
        }

        public int Index(SDAI_DAObject v)
        {
            for (int i = 0; i < _count; ++i)
            {
                if (_buf[i] == v)
                {
                    return i;
                }
            }
            return -1;
        }

        public SDAI_DAObject retrieve(int index)
        {
            return this[index];
        }

        public SDAI_DAObject this[int index]
        {
            get
            {
                Check(index);
                _count = Math.Max(_count, index + 1);
                return _buf[index];
            }
            set
            {
                Check(index);
                _count = Math.Max(_count, index + 1);
                _buf[index] = value;
            }
        }

        public int Count()
        {
            return _count;
        }

        public bool is_empty()
        {
            return _count == 0;
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}