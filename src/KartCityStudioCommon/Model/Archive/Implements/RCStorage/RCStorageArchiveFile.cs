using KartCityStudio.Common.Model.Archive.Implements.DataSource;
using KartCityStudio.Common.Model.Archive.Implements.RCStorage;
using KartCityStudio.Game.Model;
using RayCityLibrary.File;

namespace KartCityStudio.Model.Archive.Implements.RCStorage;

public class RCStorageArchiveFile: IArchiveFile
{
    private RaycityStorageFile baseRCStorageFile;

    public RaycityStorageFile BaseFile => baseRCStorageFile;

    public string Name => baseRCStorageFile.Name;

    public string FullName => baseRCStorageFile.FullName;

    public int FileSize => baseRCStorageFile.Size;

    public IArchiveFolder? Parent =>
        baseRCStorageFile?.Parent is not null
            ? new RCStorageArchiveFolderModel(baseRCStorageFile?.Parent)
            : null;

    public IDataSourceModel DataSource
    {
        set => baseRCStorageFile.DataSource = new KCSDataSource(value);
    }

    public RCStorageArchiveFile(RaycityStorageFile file)
    {
        baseRCStorageFile = file;
    }

    public Stream CreateStream() => baseRCStorageFile.CreateStream();

}
