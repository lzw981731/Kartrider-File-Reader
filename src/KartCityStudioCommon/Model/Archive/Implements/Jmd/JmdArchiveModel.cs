using KartCityStudio.Common.Model.Archive.Implements.Rho;
using KartCityStudio.Common.Service;
using KartCityStudio.Common.Service.Implements;
using KartCityStudio.Game.Model;
using KartLibrary.File;
using RayCityLibrary.File;

namespace KartCityStudio.Common.Model.Archive.Implements.Jmd;

public class JmdArchiveModel: IArchiveModel
{
    private JmdArchive? baseArchive = null;
    private string rhoArchivePath;

    public IArchiveFolder RootFolder => baseArchive is not null ? new JmdArchiveFolderModel(baseArchive.RootFolder) : throw new NullReferenceException();

    public IMusicPreviewService MusicPreviewService { get; }

    public event LoadCompletedDelegate? LoadCompleted;
    
    public event LoadFailureDelegate? LoadFailure;

    public JmdArchiveModel(string path)
    {
        rhoArchivePath = path;
        MusicPreviewService = new DefaultMusicPreviewService(this);
    }

    public void BeginLoadArchive(CancellationToken cancellationToken = default)
    {
        baseArchive = new JmdArchive();
        try
        {
            Task backgroundWorker = new Task(() =>
            {
                baseArchive.Open(rhoArchivePath);
                LoadCompleted?.Invoke(true);
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
            baseArchive = new JmdArchive();
            baseArchive.Open(rhoArchivePath);
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
        baseArchive?.Dispose();
    }
}
