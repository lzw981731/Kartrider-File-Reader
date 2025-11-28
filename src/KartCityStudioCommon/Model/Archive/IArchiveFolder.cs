using System.Collections.Generic;

namespace KartCityStudio.Game.Model;

public interface IArchiveFolder: IArchiveElement
{
    IEnumerable<IArchiveFolder> Folders { get; }

    IEnumerable<IArchiveFile> Files { get; }

    void AddFileFromPhysical(string srcFilePath, string newFileName);
}
