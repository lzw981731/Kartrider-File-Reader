using System.IO;

namespace KartCityStudio.Game.Model;

public interface IArchiveFile: IArchiveElement
{
    int FileSize { get; }

    IDataSourceModel DataSource { set; }

    Stream CreateStream();
}
