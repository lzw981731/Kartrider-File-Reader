using System.Collections;

namespace KartLibrary.File.PackFolder;

public class PackFileNode: IPackFolderNode
{
    private List<IPackFolderNode> _children = new List<IPackFolderNode>();
    
    public string Name { get; set; } = "";

    public bool LoadPass { get; set; } = false;

    public ICollection<IPackFolderNode> Children => _children;
}