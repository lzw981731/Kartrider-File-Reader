using KartCity.Common.Client;
using KartCityStudio.Common.Service;
using KartCityStudio.Common.Service.Implements;
using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.KartStorage;

public class KartStorageArchiveModel: IArchiveModel
{
    private KartStorageSystem? storageSystem;

    public KartStorageSystem? BaseArchive => storageSystem;

    public IMusicPreviewService MusicPreviewService { get; private set; }

    private string dataPath;

    public IArchiveFolder RootFolder => storageSystem is not null ? new KartStorageArchiveFolderModel(storageSystem.RootFolder) : throw new NullReferenceException();

    public event LoadCompletedDelegate? LoadCompleted;
    
    public event LoadFailureDelegate? LoadFailure;

    public KartStorageArchiveModel(string path)
    {
        dataPath = path;
    }

    public void BeginLoadArchive(CancellationToken cancellationToken = default)
    {
        KartStorageSystemBuilder builder = new KartStorageSystemBuilder()
            .UseRho()
            .UseRho5()
            .SetDataPath(dataPath)
            .SetClientRegion(CountryCode.CN);

        if (File.Exists(Path.Combine(dataPath, "aaa.pk")))
            builder.UsePackFolderListFile();

        storageSystem = builder
            .Build();

        try
        {
            Task backgroundWorker = new Task(() =>
            {
                bool result = LoadArchive(cancellationToken);
                LoadCompleted?.Invoke(result);
            });
            backgroundWorker.Start();

        }
        catch(Exception ex)
        {
            LoadFailure?.Invoke(ex);
            LoadCompleted?.Invoke(false);
        }
    }
    
    public bool LoadArchive(CancellationToken cancellationToken = default)
    {
        KartStorageSystemBuilder builder = new KartStorageSystemBuilder()
            .UseRho()
            .UseRho5()
            .SetDataPath(dataPath);

        if (File.Exists(Path.Combine(dataPath, "aaa.pk")))
            builder.UsePackFolderListFile();

        storageSystem = builder
            .Build();
        
        try
        {
            storageSystem.Initialize();
            MusicPreviewService = new KartMusicPreviewService(this);
            return true;
        }
        catch(Exception ex)
        {
            LoadFailure?.Invoke(ex);
            return false;
        }
    }

    public void Dispose()
    {
        BaseArchive?.Dispose();
    }
}
