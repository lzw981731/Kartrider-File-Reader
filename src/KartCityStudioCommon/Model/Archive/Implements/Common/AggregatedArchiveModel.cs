using KartCityStudio.Common.Service;
using KartCityStudio.Common.Service.Implements;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive.Implements.Common;

public class AggregatedArchiveModel: IArchiveModel
{
    public void Dispose()
    {
        foreach(var item in _mountArchives.Values)
            item.Dispose();
    }

    public event LoadCompletedDelegate? LoadCompleted;
    
    public event LoadFailureDelegate? LoadFailure;
    
    public IArchiveFolder RootFolder => _rootFolder;

    public IMusicPreviewService MusicPreviewService { get; }
    
    private readonly Dictionary<string, IArchiveModel> _mountArchives = [];
    
    private readonly AggregatedArchiveFolder _rootFolder = new AggregatedArchiveFolder();

    public AggregatedArchiveModel()
    {
        MusicPreviewService = new DefaultMusicPreviewService(this);
    }
    
    public void BeginLoadArchive(CancellationToken cancellationToken = default)
    {
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
        try
        {
            foreach (var mountArchive in _mountArchives)
            {
                if(LoadFailure is not null)
                    mountArchive.Value.LoadFailure += LoadFailure;
                
                bool result = mountArchive.Value.LoadArchive(cancellationToken);
                if (!result)
                {
                    Dispose();
                    return false;
                }
                _rootFolder.MountFolder(mountArchive.Key, mountArchive.Value.RootFolder);
            }
            return true;
        }
        catch(Exception ex)
        {
            LoadFailure?.Invoke(ex);
            return false;
        }
    }

    public void MountArchive(string name, IArchiveModel archive)
    {
        _mountArchives.Add(name, archive);
    }
}