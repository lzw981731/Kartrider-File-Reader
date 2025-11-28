using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.KartStorage;

public class KartStorageArchiveFolderModel: IArchiveFolder
{
    private KartStorageFolder baseKartStorageFolder;

    public IArchiveFolder? Parent =>
        baseKartStorageFolder?.Parent is not null
            ? new KartStorageArchiveFolderModel(baseKartStorageFolder?.Parent)
            : null;
    public string Name => baseKartStorageFolder.Name;
    public string FullName => baseKartStorageFolder.FullName;

    public IEnumerable<IArchiveFolder> Folders =>
        baseKartStorageFolder.Folders.Select(x => new KartStorageArchiveFolderModel(x));

    public IEnumerable<IArchiveFile> Files => baseKartStorageFolder.Files.Select(x => new KartStorageArchiveFile(x));

    public KartStorageArchiveFolderModel(KartStorageFolder storageFolder)
    {
        baseKartStorageFolder = storageFolder;
    }

    public void AddFileFromPhysical(string srcFilePath, string newFilePath)
    {
        
    }
}
