using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive.Implements.DataSource;

public class FileDataSourceModel: IDataSourceModel
{
    private FileInfo baseFileInfo;

    public bool IsLocked { get; private set; } = false;

    public long Size => baseFileInfo.Length;

    public FileStream? baseStream;

    public FileDataSourceModel(string path)
    {
        baseFileInfo = new FileInfo(path);
    }

    public bool Lock()
    {
        if (baseStream is null || !baseStream.CanRead)
        {
            baseStream?.Dispose();
            baseStream = baseFileInfo.OpenRead();
            bool result = baseStream?.CanRead ?? false;
            IsLocked = result;
            return result;
        }
        IsLocked = true;
        return IsLocked;
    }

    public void Unlock()
    {
        baseStream?.Dispose();
        IsLocked = false;
    }

    public Stream CreateStream()
    {
        if (!IsLocked || baseStream is null)
            throw new Exception("Required to lock data source model first.");
        return new FileStream(baseStream.SafeFileHandle, FileAccess.Read);
    }
}
