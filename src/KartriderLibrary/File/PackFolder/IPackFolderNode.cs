using System.Collections;

namespace KartLibrary.File.PackFolder;

public interface IPackFolderNode
{
    string Name { get; set; }
    
    ICollection<IPackFolderNode> Children { get; }
}