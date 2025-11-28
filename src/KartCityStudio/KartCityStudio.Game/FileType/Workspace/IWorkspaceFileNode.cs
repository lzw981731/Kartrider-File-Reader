using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartCityStudio.Game.FileType.Workspace;

public interface IWorkspaceFileNode
{
    string NodeName { get; }

    BinaryXmlTag ToXmlTag();
}
