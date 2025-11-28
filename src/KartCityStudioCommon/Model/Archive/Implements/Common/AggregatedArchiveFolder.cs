using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive.Implements.Common;

public class AggregatedArchiveFolder: IArchiveFolder
{
    // For Root folder.

    public string Name { get; set; } = "";

    public string FullName => Parent?.FullName.Length > 0 ? $"{Parent?.FullName}/{Name}" : Name;

    public IArchiveFolder? Parent { get; private set; }

    public IEnumerable<IArchiveFolder> Folders => RawArchiveFolder?.Folders ?? _folders.Values;

    public IEnumerable<IArchiveFile> Files => RawArchiveFolder?.Files ?? [];
    
    public IArchiveFolder? RawArchiveFolder { get; set; }
    
    private Dictionary<string, IArchiveFolder> _folders = [];
    
    public void AddFileFromPhysical(string srcFilePath, string newFileName)
    {
        throw new NotImplementedException();
    }

    public void MountFolder(string folderMountName, IArchiveFolder rawFolder)
    {
        if(!_folders.TryAdd(folderMountName, 
               new AggregatedArchiveFolder()
               {
                    Name = folderMountName,
                    RawArchiveFolder = rawFolder,   
               }
           )
        )
            throw new Exception("Folder already exists");
    }
}