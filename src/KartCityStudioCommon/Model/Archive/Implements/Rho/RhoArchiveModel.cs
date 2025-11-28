using KartCityStudio.Common.Service;
using KartCityStudio.Common.Service.Implements;
using KartCityStudio.Game.Model;
using KartLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Rho;

public class RhoArchiveModel: IArchiveModel
{
    private RhoArchive? baseArchive = null;
    private string rhoArchivePath;

    public IArchiveFolder RootFolder => baseArchive is not null ? new RhoArchiveFolderModel(baseArchive.RootFolder) : throw new NullReferenceException();

    public IMusicPreviewService MusicPreviewService { get; }

    public event LoadCompletedDelegate? LoadCompleted;
    
    public event LoadFailureDelegate? LoadFailure;

    public RhoArchiveModel(string path)
    {
        rhoArchivePath = path;
        MusicPreviewService = new DefaultMusicPreviewService(this);
    }

    public void BeginLoadArchive(CancellationToken cancellationToken = default)
    {
        baseArchive = new RhoArchive();
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
        baseArchive = new RhoArchive();
        try
        {
            baseArchive.Open(rhoArchivePath);
            return true;
        }
        catch (Exception ex)
        {
            LoadFailure?.Invoke(ex);
            return false;
        }
    }

    public void Dispose()
    {
        baseArchive?.Dispose();
    }
}
