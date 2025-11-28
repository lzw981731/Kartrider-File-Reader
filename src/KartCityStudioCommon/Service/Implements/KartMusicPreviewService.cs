using System.Text;
using System.Xml.Linq;
using KartCity.Common.Client;
using KartCity.Common.Xml;
using KartCityStudio.Common.Model.Archive.Implements.KartStorage;
using KartCityStudio.Game.Model;
using KartLibrary.File;
using KartLibrary.Game.Localization;

namespace KartCityStudio.Common.Service.Implements;

public class KartMusicPreviewService: IMusicPreviewService
{
    private KartStorageArchiveModel baseArchiveModel;

    private StringBag trackStringBag;

    private Dictionary<string, string> trackPaths;

    private CountryCode _activeCountryCode = CountryCode.TW;

    public KartMusicPreviewService(KartStorageArchiveModel archiveModel)
    {
        baseArchiveModel = archiveModel;
        trackStringBag = new StringBag();
        trackPaths = new Dictionary<string, string>();
        initStringBag();
    }

    private void initStringBag()
    {
        if (baseArchiveModel.BaseArchive is null)
            return;
        KartStorageSystem storageSystem = baseArchiveModel.BaseArchive;
        KartStorageFile? bgmLocaleFile = storageSystem?.GetFile("etc_/bgmList.xml")
                                         ?? storageSystem?.GetFile("etc_/bgmList.bml")
                                         ?? storageSystem?.GetFile("etc/bgmList.xml")
                                         ?? storageSystem?.GetFile("etc/bgmList.bml");
        if (bgmLocaleFile is not null)
        {
            using Stream bgmLocaleXmlDataStream = bgmLocaleFile.CreateStream();
            if (bgmLocaleFile.Name.EndsWith(".xml"))
            {
                XElement bgmStringBagXml = XElement.Load(bgmLocaleXmlDataStream);
                trackStringBag.LoadFromXElement(bgmStringBagXml);
            }
            else if (bgmLocaleFile.Name.EndsWith(".bml"))
            {
                BinaryReader bmlReader = new BinaryReader(bgmLocaleXmlDataStream);
                BinaryXmlTag binaryXmlTag = bmlReader.ReadBinaryXmlTag(Encoding.Unicode);
                trackStringBag.LoadFromBinaryXmlTag(binaryXmlTag);
            }

            trackStringBag.SetString(CountryCode.TW, "maple_05", "夢之都拉契爾恩");
            trackStringBag.SetString(CountryCode.TW, "maple_06", "夢境碎片");
            trackStringBag.SetString(CountryCode.TW, "mu_garage_sunmr", "Raycity 車庫背景音樂");
        }
    }

    private string displayName(string trackName)
    {
        return _activeCountryCode switch
        {
            CountryCode.KR => trackStringBag.GetString(CountryCode.KR, trackName, defaultIfNull: false),
            CountryCode.CN => trackStringBag.GetString(CountryCode.CN, trackName, defaultIfNull: false) ??
                              trackStringBag.GetString(CountryCode.TW, trackName, defaultIfNull: false) ??
                              trackStringBag.GetString(CountryCode.KR, trackName, defaultIfNull: false),
            CountryCode.TW => trackStringBag.GetString(CountryCode.TW, trackName, defaultIfNull: false) ??
                              trackStringBag.GetString(CountryCode.CN, trackName, defaultIfNull: false) ??
                              trackStringBag.GetString(CountryCode.KR, trackName, defaultIfNull: false),
            _ => trackStringBag.GetString(CountryCode.None, trackName)
        } ?? trackName;
    }

    public MusicPreviewInfo GetMusicPreviewInfo(IArchiveFile musicFile)
    {
        MusicPreviewInfo previewInfo = new MusicPreviewInfo();
        previewInfo.TrackName = musicFile.Name;
        if (musicFile is KartStorageArchiveFile storageArchiveFile)
        {
            previewInfo.TrackName = displayName(storageArchiveFile.BaseFile.NameWithoutExt);
        }
        return previewInfo;
    }
}
