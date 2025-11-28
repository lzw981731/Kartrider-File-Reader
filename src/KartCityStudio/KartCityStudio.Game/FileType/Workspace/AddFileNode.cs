using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartCityStudio.Game.FileType.Workspace;

public class AddFileNode: IWorkspaceFileNode
{
    public string NodeName => "add-file";

    public string From { get; set; } = "";

    public string To { get; set; } = "";

    public BinaryXmlTag ToXmlTag()
    {

        return new BinaryXmlTag(NodeName)
            .SetAttributeContinue("from", From)
            .SetAttributeContinue("to", To);
    }
}
