using KartCityStudio.Game.Model;
using KartCityStudio.Model.Archive.Implements.RCStorage;
using RayCityLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.RCStorage;

public class RCStorageArchiveFolderModel: IArchiveFolder
{
    private RaycityStorageFolder baseRCStorageFolder;

    public IArchiveFolder? Parent =>
        baseRCStorageFolder?.Parent is not null
            ? new RCStorageArchiveFolderModel(baseRCStorageFolder?.Parent)
            : null;

    public string Name => baseRCStorageFolder.Name;

    public string FullName => baseRCStorageFolder.FullName;

    public IEnumerable<IArchiveFolder> Folders =>
        baseRCStorageFolder.Folders.Select(x => new RCStorageArchiveFolderModel(x));

    public IEnumerable<IArchiveFile> Files => baseRCStorageFolder.Files.Select(x => new RCStorageArchiveFile(x));

    public RCStorageArchiveFolderModel(RaycityStorageFolder storageFolder)
    {
        baseRCStorageFolder = storageFolder;
    }

    public void AddFileFromPhysical(string srcFilePath, string newFilePath)
    {

    }
}
