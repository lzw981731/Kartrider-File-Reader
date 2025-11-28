using KartCityStudio.Common.Model.Archive;
using KartCityStudio.Game.Model;

namespace KartCityStudio.Common.Service.Implements;

public class DefaultMusicPreviewService: IMusicPreviewService
{
    private IArchiveModel baseModel;

    public DefaultMusicPreviewService(IArchiveModel baseModel)
    {
        this.baseModel = baseModel;
    }

    public MusicPreviewInfo GetMusicPreviewInfo(IArchiveFile musicFile)
    {
        MusicPreviewInfo previewInfo = new MusicPreviewInfo();
        previewInfo.TrackName = musicFile.Name;
        return previewInfo;
    }
}
