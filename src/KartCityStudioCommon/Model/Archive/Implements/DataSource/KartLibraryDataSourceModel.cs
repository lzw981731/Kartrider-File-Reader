using KartCity.Common.FileType;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive.Implements.DataSource;

/// <summary>
/// IDataSource -> IDataSourceModel
/// </summary>
public class KartLibraryDataSourceModel: IDataSourceModel
{
    private IDataSource baseDataSource;

    // IDataSource is not support lock operation currently.
    public bool IsLocked => true;

    public long Size => baseDataSource.Size;

    public KartLibraryDataSourceModel(IDataSource baseDataSource)
    {
        this.baseDataSource = baseDataSource;
    }

    public bool Lock()
    {
        return true;
    }

    public void Unlock()
    {

    }

    public Stream CreateStream()
    {
        return baseDataSource.CreateStream();
    }
}
