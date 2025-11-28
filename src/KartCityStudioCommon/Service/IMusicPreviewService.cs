using KartCity.Common.FileType;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Service;

public interface IMusicPreviewService
{
    MusicPreviewInfo GetMusicPreviewInfo(IArchiveFile musicFile);
}

public class MusicPreviewInfo
{
    public string TrackName { get; set; } = "";

    public IRhoFile? BackgroundImage { get; set; }
}
