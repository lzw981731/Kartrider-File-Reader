using KartLibrary.File;

namespace KartLibrary.Game.Track;

public class TrackManager
{
    private KartStorageSystem _storageSystem;

    public TrackManager(KartStorageSystem storageSystem)
    {
        KartStorageFolder? commonFolder =
            storageSystem.GetFolder("kart/common") ?? storageSystem.GetFolder("kart_/common");
        if (commonFolder is null)
            throw new Exception("Can't initialize TrackManager!");
        
    }
}