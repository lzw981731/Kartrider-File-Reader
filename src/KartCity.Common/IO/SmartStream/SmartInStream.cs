using KartLibrary.IO;

namespace KartCity.Common.IO.SmartStream;

/// <summary>
/// SmartStream is an object that can auto compress/encrypt stream data.
/// </summary>
public class SmartInStream: Stream
{
    private byte[] _buffer = new byte[0];

    private bool _decoded = false;

    private int _position = 0;

    private int _length = 0;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set
        {
            if (value > _buffer.Length || value < 0)
                throw new Exception("Invalid position.");
            _position = (int)value;
        }
    }

    public SmartInStream(byte[] orgData)
    {
        if (orgData.Length < 0x02 || orgData[0] != 0x53)
            throw new Exception("Given data isn't smart stream data.");
        SmartStreamMode mode = (SmartStreamMode)orgData[1];
        int minLength = mode switch
        {
            SmartStreamMode.None => 6,
            SmartStreamMode.Compressed => 10,
            SmartStreamMode.Encrypted => 10,
            SmartStreamMode.CompressedEncrypted => 14,
            _ => 0
        };
        if(orgData.Length < minLength)
            throw new Exception("Given data isn't smart stream data.");
        _length =
            mode.HasFlag(SmartStreamMode.Compressed)
                ? BitConverter.ToInt32(orgData, minLength - 4)
                : orgData.Length - minLength;
        _buffer = orgData;
        _decoded = false;
    }
    
    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!_decoded)
        {
            _buffer = SmartStreamUtility.DecodeSmartStreamData(_buffer);
            _decoded = true;
            _length = _buffer.Length;
        }

        int readCount = Math.Min(_buffer.Length - _position, count);
        Array.Copy(_buffer, _position, buffer, offset, count);
        _position += readCount;
        return readCount;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _length - offset,
            _ => -1
        };
        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}