using System.Text;
using KartCity.Common.FileType;
using KartCity.Common.IO;
using KartCity.Common.Xml;
using KartCityStudio.Game.Model;
using KartCityStudio.Model.Archive.Implements.RCStorage;
using RayCityLibrary.File;

namespace KartCityStudio.Common.Service.Implements;

public class RCMusicPreviewService: IMusicPreviewService
{
    private RCStorageArchiveModel baseArchiveModel;

    private Dictionary<string, Dictionary<string, string>> trackNameTable = new Dictionary<string, Dictionary<string, string>>();

    private readonly string[] otherBackgrounds = new[]
    {
        "ui/loginstage/01_a.jpg",
        "ui/loginstage/02_a.jpg",
        "ui/loginstage/03_a.jpg",
        "ui/loginstage/04_a.jpg",
        "ui/loginstage/05_a.jpg",
        "ui/loginstage/06_a.jpg",
        "ui/loginstage/07_a.jpg",
    };

    private string[] availableOtherBackgrounds;

    public RCMusicPreviewService(RCStorageArchiveModel archiveModel)
    {
        baseArchiveModel = archiveModel;
        availableOtherBackgrounds = otherBackgrounds.Where(
            x => baseArchiveModel.BaseStorageSystem?.RootFolder.GetFile(x) is not null)
            .ToArray();
    }

    public MusicPreviewInfo GetMusicPreviewInfo(IArchiveFile musicFile)
    {
        MusicPreviewInfo previewInfo = new MusicPreviewInfo();
        previewInfo.TrackName = musicFile.Name;
        IArchiveFile? listFile = musicFile.Parent?.Files.FirstOrDefault(x => x.Name == "list.xml");
        if (listFile is RCStorageArchiveFile rcStorageArchiveFile)
        {
            BinaryXmlTag listXml = rcStorageArchiveFile.BaseFile.ReadXml();
            if (listXml.Name.ToLower() == "bgmlist")
            {
                foreach (var bgmTag in listXml["bgm"])
                {
                    string? name = bgmTag.GetAttribute("name");
                    if(name is null)
                        continue;

                    string title = bgmTag.GetAttribute("title") ?? "";
                    string singer = bgmTag.GetAttribute("singer") ?? "";
                    if (name.ToLower() == musicFile.Name)
                    {
                        previewInfo.TrackName = $"{singer} - {title}";
                        break;
                    }
                }
            }
        }

        if (musicFile.Name == "mu_bgm_org_driversparadise.ogg")
        {
            RaycityStorageFile? darkcityImgFile = baseArchiveModel.BaseStorageSystem?.RootFolder.GetFile("ui/loginstage/darkcity.jpg");
            if (darkcityImgFile is not null)
                previewInfo.BackgroundImage = darkcityImgFile;
        }
        else if (musicFile.Name.ToLower().StartsWith("darkcity_"))
        {
            RaycityStorageFile? darkcityImgFile = baseArchiveModel.BaseStorageSystem?.RootFolder.GetFile("ui/loadingstage/tofield_d.jpg");
            if (darkcityImgFile is not null)
                previewInfo.BackgroundImage = darkcityImgFile;
        }
        else if (musicFile.Name == "mu_garage_sunmr.ogg")
        {
            RaycityStorageFile? darkcityImgFile = baseArchiveModel.BaseStorageSystem?.RootFolder.GetFile("ui/loadingstage/event02.jpg");
            if (darkcityImgFile is not null)
                previewInfo.BackgroundImage = darkcityImgFile;
        }

        if (previewInfo.BackgroundImage is null && availableOtherBackgrounds.Length > 0)
        {
            uint adlerHashedName = Adler.Adler32(0, Encoding.Unicode.GetBytes(musicFile.Name), 0, musicFile.Name.Length << 1);
            string selectedFile =
                availableOtherBackgrounds[(adlerHashedName * 4599739) % availableOtherBackgrounds.Length];
            RaycityStorageFile? darkcityImgFile = baseArchiveModel.BaseStorageSystem?.RootFolder.GetFile(selectedFile);
            if (darkcityImgFile is not null)
                previewInfo.BackgroundImage = darkcityImgFile;
        }
        return previewInfo;
    }
}
