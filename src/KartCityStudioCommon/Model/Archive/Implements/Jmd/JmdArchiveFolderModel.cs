using KartCityStudio.Common.Model.Archive.Implements.Rho;
using KartCityStudio.Game.Model;
using KartLibrary.File;
using RayCityLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Jmd;

public class JmdArchiveFolderModel: IArchiveFolder
{
    private JmdFolder baseJmdFolder;

    public IArchiveFolder? Parent =>
        baseJmdFolder?.Parent is not null
            ? new JmdArchiveFolderModel(baseJmdFolder?.Parent)
            : null;
    public string Name => baseJmdFolder.Name;
    public string FullName => baseJmdFolder.FullName;

    public IEnumerable<IArchiveFolder> Folders =>
        baseJmdFolder.Folders.Select(x => new JmdArchiveFolderModel(x));

    public IEnumerable<IArchiveFile> Files => baseJmdFolder.Files.Select(x => new JmdArchiveFile(x));

    public JmdArchiveFolderModel(JmdFolder folder)
    {
        baseJmdFolder = folder;
    }

    public void AddFileFromPhysical(string srcFilePath, string newFilePath)
    {

    }
}
