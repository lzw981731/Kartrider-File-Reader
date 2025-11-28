using System.Collections;
using System.Collections.ObjectModel;

namespace KartLibrary.File.PackFolder;

public class RhoFolderNode: IPackFolderNode
{
    public string Name { get; set; } = "";

    public string FileName { get; set; } = "";
    
    public uint Key { get; set; }
    
    public uint DataHash { get; set; }
    
    public int MediaSize { get; set; }

    public ICollection<IPackFolderNode> Children => throw new Exception();
}