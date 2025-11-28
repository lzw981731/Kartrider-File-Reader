using KartCity.Common.FileType;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive.Implements.DataSource;

/// <summary>
/// <see cref="KCSDataSource"/> is not DataSourceModel.
/// It implements <see cref="IDataSource"/> for IDataSourceModel.
/// </summary>
public class KCSDataSource: IDataSource
{
    private IDataSourceModel baseModel;

    public KCSDataSource(IDataSourceModel sourceModel)
    {
        baseModel = sourceModel;
    }

    public bool Locked => baseModel.IsLocked;
    public int Size => (int)baseModel.Size;
    public Stream CreateStream()
    {
        if (!baseModel.IsLocked && baseModel.Lock())
            throw new Exception("Can't lock data source model.");
        return baseModel.CreateStream();
    }

    public void Dispose()
    {
        if(baseModel.IsLocked)
            baseModel.Unlock();
    }

    public void WriteTo(Stream stream)
    {
        baseModel.CreateStream().CopyTo(stream);
    }

    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await baseModel.CreateStream().CopyToAsync(stream, cancellationToken);
    }

    public void WriteTo(byte[] array, int offset, int count)
    {
        baseModel.CreateStream().Write(array, offset, count);
    }

    public async Task WriteToAsync(byte[] array, int offset, int count, CancellationToken cancellationToken = default)
    {
        await baseModel.CreateStream().WriteAsync(array, offset, count, cancellationToken);
    }

    public byte[] GetBytes()
    {
        byte[] buffer = new byte[Size];
        WriteTo(buffer, 0, buffer.Length);
        return buffer;
    }

    public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[Size];
        await WriteToAsync(buffer, 0, buffer.Length, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return [];
        return buffer;
    }
}
