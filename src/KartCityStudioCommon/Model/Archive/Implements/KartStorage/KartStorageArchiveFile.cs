using KartCityStudio.Common.Model.Archive.Implements.DataSource;
using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.KartStorage;

public class KartStorageArchiveFile: IArchiveFile
{
    private KartStorageFile baseKartStorageFile;

    public KartStorageFile BaseFile => baseKartStorageFile;

    public string Name => baseKartStorageFile.Name;

    public string FullName => baseKartStorageFile.FullName;

    public int FileSize => baseKartStorageFile.Size;

    public IArchiveFolder? Parent =>
        baseKartStorageFile?.Parent is not null
            ? new KartStorageArchiveFolderModel(baseKartStorageFile?.Parent)
            : null;

    public IDataSourceModel DataSource
    {
        set => baseKartStorageFile.DataSource = new KCSDataSource(value);
    }

    public KartStorageArchiveFile(KartStorageFile file)
    {
        baseKartStorageFile = file;
    }

    public Stream CreateStream() => baseKartStorageFile.CreateStream();

}
