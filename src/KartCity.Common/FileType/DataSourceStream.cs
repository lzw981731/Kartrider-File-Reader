namespace KartCity.Common.FileType;

/// <summary>
/// Represents a read-only stream created by CreateStream method of <see cref="IDataSource"/>.
/// </summary>
public class DataSourceStream: Stream
{
    private Stream _baseStream;
    private bool _disposed;
    
    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _baseStream.Length;

    internal event Action<DataSourceStream>? StreamDisposed;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    internal DataSourceStream(Stream baseStream)
    {
        _baseStream = baseStream;
        _disposed = false;
    }
    
    public override void Flush()
    {
        if (_disposed)
            throw new InvalidOperationException("Can't do any operation on disposed stream.");
        _baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_disposed)
            throw new InvalidOperationException("Can't do any operation on disposed stream.");
        return _baseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (_disposed)
            throw new InvalidOperationException("Can't do any operation on disposed stream.");
        return _baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _baseStream.Dispose();
            StreamDisposed?.Invoke(this);
        }
        _disposed = true;
    }
}