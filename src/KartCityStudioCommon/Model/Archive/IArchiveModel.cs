using KartCityStudio.Common.Service;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Model.Archive;

/// <summary>
/// <see cref="IArchiveModel"/> is used to represent an archive model.
/// </summary>
public interface IArchiveModel: IDisposable
{
    event LoadCompletedDelegate? LoadCompleted;
    event LoadFailureDelegate? LoadFailure;

    IArchiveFolder RootFolder { get; }

    IMusicPreviewService MusicPreviewService { get; }

    /// <summary>
    /// This method will start loading archive works.
    /// After loading, raising <see cref="LoadCompleted"/> event.
    /// </summary>
    void BeginLoadArchive(CancellationToken cancellationToken = default);
    
    bool LoadArchive(CancellationToken cancellationToken = default);
}

public delegate void LoadCompletedDelegate(bool success);

public delegate void LoadFailureDelegate(Exception exception);
