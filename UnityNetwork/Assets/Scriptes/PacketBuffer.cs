using System;
using System.Collections.Generic;
using System.Text;

public class PacketBuffer : IDisposable
{
    List<byte> _bufferlists;
    byte[] _readbuffer;
    int _readpos;
    bool _buffupdate = false;

    //Constructor
    public PacketBuffer()
    {
        _bufferlists = new List<byte>();
        _readpos = 0;
    }
    public int GetReadPos()
    {
        return _readpos;
    }
    public byte[] ToArray()
    {
        return _bufferlists.ToArray();
    }
    public int Count()
    {
        return _bufferlists.Count;
    }
    public int Length()
    {
        return Count() - _readpos;
    }
    public void Clear()
    {
        _bufferlists.Clear();
        _readpos = 0;
    }

    //Write Data 資料寫入
    public void WriteBytes(byte[] input)
    {
        _bufferlists.AddRange(input);
        _buffupdate = true;
    }
    public void WriteBytes(byte input)
    {
        _bufferlists.Add(input);
        _buffupdate = true;
    }
    public void WriteInteger(int input)
    {
        _bufferlists.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    public void WriteFloat(float input)
    {
        _bufferlists.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    public void WriteString(string input)
    {
        _bufferlists.AddRange(BitConverter.GetBytes(input.Length));
        _bufferlists.AddRange(Encoding.ASCII.GetBytes(input));
        _buffupdate = true;
    }

    //ReadData 資料讀取
    public int ReadInteger(bool peek = true)
    {
        if (_bufferlists.Count > _readpos)
        {
            if (_buffupdate)
            {
                _readbuffer = _bufferlists.ToArray();
                _buffupdate = false;
            }
            int value = BitConverter.ToInt32(_readbuffer, _readpos);
            if (peek && _bufferlists.Count > _readpos)
            {
                _readpos += 4;
            }

            return value;
        }
        else
        {
            throw new Exception("Buffer is past its Limit!");
        }
    }
    public float ReadFloat(bool peek = true)
    {
        if (_bufferlists.Count > _readpos)
        {
            if (_buffupdate)
            {
                _readbuffer = _bufferlists.ToArray();
                _buffupdate = false;
            }
            float value = BitConverter.ToSingle(_readbuffer, _readpos);
            if (peek && _bufferlists.Count > _readpos)
            {
                _readpos += 4;
            }

            return value;
        }
        else
        {
            throw new Exception("Buffer is past its Limit!");
        }
    }
    public byte ReadByte(bool peek = true)
    {
        if (_bufferlists.Count > _readpos)
        {
            if (_buffupdate)
            {
                _readbuffer = _bufferlists.ToArray();
                _buffupdate = false;
            }
            byte value = _readbuffer[_readpos];
            if (peek && _bufferlists.Count > _readpos)
            {
                _readpos += 1;
            }

            return value;
        }
        else
        {
            throw new Exception("Buffer is past its Limit!");
        }
    }
    public byte[] ReadBytes(int length, bool peek = true)
    {
        if (_buffupdate)
        {
            _readbuffer = _bufferlists.ToArray();
            _buffupdate = false;
        }
        byte[] value = _bufferlists.GetRange(_readpos, length).ToArray();
        if (peek && _bufferlists.Count > _readpos)
        {
            _readpos += length;
        }

        return value;
    }
    public string ReadString(bool peek = true)
    {
        int length = ReadInteger(true);
        if (_buffupdate)
        {
            _readbuffer = _bufferlists.ToArray();
            _buffupdate = false;
        }
        string value = Encoding.ASCII.GetString(_readbuffer, _readpos, length);
        if (peek && _bufferlists.Count > _readpos)
        {
            _readpos += length;
        }

        return value;
    }


    //IDispossable
    private bool disposedValue = false;
    protected virtual void Dispose(bool dispossing)
    {
        if (!disposedValue)
        {
            if (dispossing)
            {
                _bufferlists.Clear();
            }
            _readpos = 0;
        }
        disposedValue = true;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
