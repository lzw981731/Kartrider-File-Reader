using KartCityStudio.Common.Model.Archive.Implements.DataSource;
using KartCityStudio.Common.Model.Archive.Implements.Rho;
using KartCityStudio.Game.Model;
using KartLibrary.File;
using RayCityLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Jmd;

public class JmdArchiveFile: IArchiveFile
{
    private JmdFile _baseJmdFile;

    public JmdFile BaseFile => _baseJmdFile;
    
    public string Name => _baseJmdFile.Name;
    public string FullName => _baseJmdFile.FullName;

    public int FileSize => _baseJmdFile.Size;

    public IArchiveFolder? Parent =>
        _baseJmdFile?.Parent is not null
            ? new JmdArchiveFolderModel(_baseJmdFile?.Parent)
            : null;

    public IDataSourceModel DataSource
    {
        set => _baseJmdFile.DataSource = new KCSDataSource(value);
    }

    public JmdArchiveFile(JmdFile file)
    {
        _baseJmdFile = file;
    }

    public Stream CreateStream() => _baseJmdFile.CreateStream();
}
