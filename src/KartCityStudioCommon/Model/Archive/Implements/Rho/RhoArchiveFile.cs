using KartCityStudio.Common.Model.Archive.Implements.DataSource;
using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Rho;

public class RhoArchiveFile: IArchiveFile
{
    private RhoFile baseRhoFile;

    public RhoFile BaseFile => baseRhoFile;
    public string Name => baseRhoFile.Name;
    public string FullName => baseRhoFile.FullName;

    public int FileSize => baseRhoFile.Size;

    public IArchiveFolder? Parent =>
        baseRhoFile?.Parent is not null
            ? new RhoArchiveFolderModel(baseRhoFile?.Parent)
            : null;

    public IDataSourceModel DataSource
    {
        set => baseRhoFile.DataSource = new KCSDataSource(value);
    }

    public RhoArchiveFile(RhoFile file)
    {
        baseRhoFile = file;
    }

    public Stream CreateStream() => baseRhoFile.CreateStream();
}
