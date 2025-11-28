using KartCityStudio.Common.Model.Archive;
using KartCityStudio.Common.Model.Archive.Implements.RCStorage;
using KartCityStudio.Common.Service;
using KartCityStudio.Common.Service.Implements;
using KartCityStudio.Game.Model;
using RayCityLibrary.File;

namespace KartCityStudio.Model.Archive.Implements.RCStorage;

public class RCStorageArchiveModel: IArchiveModel
{
    private RaycityStorageSystem? storageSystem;

    public RaycityStorageSystem? BaseStorageSystem => storageSystem;

    private string dataPath;

    public IMusicPreviewService MusicPreviewService { get; private set; }

    public IArchiveFolder RootFolder => storageSystem is not null ? new RCStorageArchiveFolderModel(storageSystem.RootFolder) : throw new NullReferenceException();

    public event LoadCompletedDelegate? LoadCompleted;
    
    public event LoadFailureDelegate? LoadFailure;

    public RCStorageArchiveModel(string path)
    {
        dataPath = path;
    }

    public void BeginLoadArchive(CancellationToken cancellationToken = default)
    {
        storageSystem = new RaycityStorageSystem(dataPath);
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
            LoadCompleted?.Invoke(false);
            LoadFailure?.Invoke(ex);
        }
    }
    
    public bool LoadArchive(CancellationToken cancellationToken = default)
    {
        storageSystem = new RaycityStorageSystem(dataPath);
        try
        {
            storageSystem.Initialize();
            MusicPreviewService = new RCMusicPreviewService(this);
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
        BaseStorageSystem?.Dispose();
    }
}
