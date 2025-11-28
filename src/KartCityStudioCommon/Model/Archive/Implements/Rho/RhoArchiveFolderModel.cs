using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Rho;

public class RhoArchiveFolderModel: IArchiveFolder
{
    private RhoFolder baseRhoFolder;

    public IArchiveFolder? Parent =>
        baseRhoFolder?.Parent is not null
            ? new RhoArchiveFolderModel(baseRhoFolder?.Parent)
            : null;
    public string Name => baseRhoFolder.Name;
    public string FullName => baseRhoFolder.FullName;

    public IEnumerable<IArchiveFolder> Folders =>
        baseRhoFolder.Folders.Select(x => new RhoArchiveFolderModel(x));

    public IEnumerable<IArchiveFile> Files => baseRhoFolder.Files.Select(x => new RhoArchiveFile(x));

    public RhoArchiveFolderModel(RhoFolder folder)
    {
        baseRhoFolder = folder;
    }

    public void AddFileFromPhysical(string srcFilePath, string newFilePath)
    {

    }
}
