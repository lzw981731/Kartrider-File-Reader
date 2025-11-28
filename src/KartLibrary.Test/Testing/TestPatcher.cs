using System.Text;
using eP.Command;
using eP.Testing;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.FileType;
using KartCity.Common.IO.SmartStream;
using KartCity.Common.Xml;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Tests.Testing;

public class TestPatcher: TestStage
{
    private string _gameDataPath = "";
    private string _patchPatch => Path.Combine(_gameDataPath, "_patched");
    private HashSet<string> _koreaThemeOrder = ["1025", "0", "1024", "fengshen", "china", "maple"];
    
    public TestPatcher()
    {
        
    }

    [Command("set", "Set variable.")]
    private CommandExecuteResult commandSet(IConsole console, CommandArgumentQueue argumentQueue)
    {
        string var = argumentQueue.PopArgumentString().ToLower();
        if (var == "datapath")
        {
            _gameDataPath = argumentQueue.PopArgumentString();
            return new CommandExecuteResult(ResultType.Success, "");
        }
        else
        {
            return new CommandExecuteResult(ResultType.Failure, "Unknown variable.");
        }
    }
    
    
    [Command("patchKorea", "Enable Korea theme in china client.")]
    private CommandExecuteResult commandPatchKorea(IConsole console, CommandArgumentQueue argumentQueue)
    {
        string patchSrcFolder = argumentQueue.PopArgumentString();
        string patchDestFolder = _patchPatch;
        createPatchedFolder();
        if(!Directory.Exists(patchSrcFolder))
            return new CommandExecuteResult(ResultType.Failure, "PatchSrc doesn't exist.");
        string copyFolder = Path.Combine(patchSrcFolder, "_copy");
        string dataPack3Folder = Path.Combine(patchSrcFolder, "DataPack3");
        string trackCommon = Path.Combine(patchSrcFolder, "track_common");
        string trackThumb = Path.Combine(patchSrcFolder, "trackThumb");

        uint trackCommonHash = 0;
        uint trackThumbHash = 0;
        
        int trackCommonLength = 0;
        int trackThumbLength = 0;
       
        // Add Tracks
        using (Rho5Archive trackFile = new Rho5Archive())
        {
            trackFile.Open(_gameDataPath, "DataPack3", CountryCode.CN);
            mountToRhoFolder<Rho5Folder, Rho5File>(trackFile.RootFolder, new DirectoryInfo(dataPack3Folder));
            trackFile.Save(patchDestFolder, "DataPack3", CountryCode.CN, SavePattern.AlwaysRegeneration);
        }
        
        // Patch Track Locale
        using (RhoArchive trackCommonFile = new RhoArchive())
        {
            trackCommonFile.Open(Path.Combine(_gameDataPath, "track_common.rho"));
            RhoFile? trackLocaleFile =
                trackCommonFile.RootFolder.GetFile("trackLocale@cn.bml") ??
                trackCommonFile.RootFolder.GetFile("trackLocale@cn.xml");
            if(trackLocaleFile is null)
                return new CommandExecuteResult(ResultType.Failure, "Can't find TrackLocale.");
            BinaryXmlTag localeXml = trackLocaleFile.ReadXml(isBinaryFormat: trackLocaleFile.Name.EndsWith("bml"));
            localeXml.AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_I01")
                        .SetAttributeContinue("name", "韓國 全州韓屋村")
                        .SetAttributeContinue("bpLevelMin", "15")
                        .SetAttributeContinue("bpLevelMax", "29")
                        .SetAttributeContinue("basicAi", "true")
                ).AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_I02")
                        .SetAttributeContinue("name", "韓國 樂天世界大冒險")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_I03")
                        .SetAttributeContinue("name", "韓國 仁川中央公園")
                        .SetAttributeContinue("bpLevelMin", "15")
                        .SetAttributeContinue("bpLevelMax", "29")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_I04")
                        .SetAttributeContinue("name", "韓國 韓國民俗村")
                        .SetAttributeContinue("bpLevelMin", "0")
                        .SetAttributeContinue("bpLevelMax", "22")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_R01")
                        .SetAttributeContinue("name", "韓國 繁華的首爾")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_R02")
                        .SetAttributeContinue("name", "韓國 釜山的夜晚")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_R03")
                        .SetAttributeContinue("name", "韓國 濟州日出下坡路")
                        .SetAttributeContinue("basicAi", "true")
                )
                .AddContinue(
                    new BinaryXmlTag("track")
                        .SetAttributeContinue("id", "korea_R04")
                        .SetAttributeContinue("name", "韓國 釜山夜海的逆行")
                        .SetAttributeContinue("blocked", "true")
                        .SetAttributeContinue("basicAi", "false")
                );
            
            trackLocaleFile.DataSource = 
                trackLocaleFile.Name.EndsWith("xml") 
                    ? new ByteArrayDataSource(Encoding.Unicode.GetBytes(localeXml.ToString())) 
                    : new ByteArrayDataSource(localeXml.ToBinary(Encoding.Unicode));
            
            trackCommonFile.SaveTo(Path.Combine(patchDestFolder, "track_common.rho"));

            trackCommonHash = trackCommonFile.DataHash;
            trackCommonLength = trackCommonFile.Size;
        }
        
        // Add Track Thumb
        using (RhoArchive trackThumbFile = new RhoArchive())
        {
            trackThumbFile.Open(Path.Combine(_gameDataPath, "trackThumb.rho"));
            mountToRhoFolder<RhoFolder, RhoFile>(trackThumbFile.RootFolder, new DirectoryInfo(trackThumb));
            trackThumbFile.SaveTo(Path.Combine(patchDestFolder, "trackThumb.rho"));

            trackThumbHash = trackThumbFile.DataHash;
            trackThumbLength = trackThumbFile.Size;
        }
        
        // Patch select track dialog
        uint dialog2Hash = 0;
        int dialog2Size = 0;
        using (RhoArchive dialog2File = new RhoArchive())
        {
            dialog2File.Open(Path.Combine(_gameDataPath, "dialog2_selectTrackEx.rho"));
            RhoFile? configFile =
                dialog2File.RootFolder.GetFile("config@cn.bml") ??
                dialog2File.RootFolder.GetFile("config@cn.xml");
            if(configFile is null)
                return new CommandExecuteResult(ResultType.Failure, "Can't find TrackLocale.");
            BinaryXmlTag configXml = configFile.ReadXml(isBinaryFormat: configFile.Name.EndsWith("bml"));
            BinaryXmlTag? themeTabOrder = configXml["themeTabOrder"].FirstOrDefault();
            if (themeTabOrder is not null)
            {
                int insertIndex = 0;
                int curIndex = 0;
                foreach (BinaryXmlTag themeTag in themeTabOrder["theme"])
                {
                    if (_koreaThemeOrder.Contains(((string?)themeTag.GetAttribute("id"))?.ToLower()))
                        insertIndex = Math.Max(insertIndex, curIndex);
                    curIndex++;
                }
                themeTabOrder.Children.Insert(insertIndex, new BinaryXmlTag("theme").SetAttributeContinue("id", "korea"));
            }
            configFile.DataSource = 
                configXml.Name.EndsWith("xml") 
                    ? new ByteArrayDataSource(Encoding.Unicode.GetBytes(configXml.ToString())) 
                    : new ByteArrayDataSource(configXml.ToBinary(Encoding.Unicode));
            dialog2File.SaveTo(Path.Combine(patchDestFolder, "dialog2_selectTrackEx.rho"));

            dialog2Hash = dialog2File.DataHash;
            dialog2Size = dialog2File.Size;
        }
        
        // Patch bgmList and baseStringBag
        using (Rho5Archive etcFile = new Rho5Archive())
        {
            etcFile.Open(_gameDataPath, "DataPack1", CountryCode.CN);
            Rho5File? bgmListFile = etcFile.RootFolder.GetFile("etc_/bgmList.xml");
            if(bgmListFile is null)
                return new CommandExecuteResult(ResultType.Failure, "Can't find bgmList file.");
            BinaryXmlTag bgmListXml = bgmListFile.ReadXml(isBinaryFormat: false);
            Dictionary<string, string> translations = new Dictionary<string, string>()
            {
                ["korea_01"] = "欢迎来韩国",
                ["korea_02"] = "东奔西跑",
                ["korea_03"] = "狐狸尾巴有九个?",
                ["korea_04"] = "灵魂驱动",
                ["korea_05"] = "山坡下的西风",
            };
            HashSet<string> unsetBgm = [..translations.Keys];
            foreach (BinaryXmlTag stringBag in bgmListXml["k"])
            {
                string currentBgmName = stringBag.GetAttribute("n") ?? "";
                if (translations.TryGetValue(currentBgmName, out var translation))
                {
                    BinaryXmlTag? valueTag = stringBag["m"].FirstOrDefault(x => x.GetAttribute("c") == "cn");
                    if (valueTag is null)
                    {
                        valueTag = new BinaryXmlTag("m").SetAttributeContinue("c", "cn");
                        stringBag.Add(valueTag);
                    }
                    valueTag.SetAttribute("v", translation);
                    unsetBgm.Remove(currentBgmName);
                }
            }

            foreach (string bgmName in unsetBgm)
            { 
                bgmListXml.Add(
                    new BinaryXmlTag("k")
                        .SetAttributeContinue("n", bgmName)
                        .AddContinue(new BinaryXmlTag("m")
                            .SetAttributeContinue("c", "cn")
                            .SetAttributeContinue("v", translations[bgmName])
                        )
                );
            }

            bgmListFile.DataSource = new ByteArrayDataSource([..Encoding.Unicode.GetPreamble(), ..Encoding.Unicode.GetBytes("<?xml version='1.0' encoding='utf-16'?>\r\n" + bgmListXml.ToString())]);

            Rho5File? baseStringBagFile = etcFile.RootFolder.GetFile("etc_/baseStringBag.xml");
            if(baseStringBagFile is null)
                return new CommandExecuteResult(ResultType.Failure, "Can't find baseStringBagFile file.");

            BinaryXmlTag baseStringBagXml = baseStringBagFile.ReadXml(isBinaryFormat: false);
            BinaryXmlTag? themeKoreaTag =
                baseStringBagXml["k"].FirstOrDefault(x => x.GetAttribute("n") == "themeKorea");
            if (themeKoreaTag is null)
                baseStringBagXml.AddContinue(
                    themeKoreaTag = new BinaryXmlTag("k")
                        .SetAttributeContinue("n", "themeKorea")
                );
            BinaryXmlTag? themeValue = themeKoreaTag["m"].FirstOrDefault(x => x.GetAttribute("c") == "cn");
            if (themeValue is null)
                themeKoreaTag.AddContinue(
                    themeValue = new BinaryXmlTag("m")
                        .SetAttributeContinue("c", "cn")
                );
            themeValue.SetAttributeContinue("v", "韩国");

            
            baseStringBagFile.DataSource =
                new ByteArrayDataSource([..Encoding.Unicode.GetPreamble(), ..Encoding.Unicode.GetBytes("<?xml version='1.0' encoding='utf-16'?>\r\n" + baseStringBagXml.ToString())]);
            etcFile.Save(patchDestFolder, "DataPack1", CountryCode.CN, SavePattern.AlwaysRegeneration);
        }
        
         // Add sounds
        using (FileStream aaaPkFile = new FileStream(Path.Combine(_gameDataPath, "aaa.pk"), FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(aaaPkFile);
            int smartStreamLen = reader.ReadInt32();
            byte[] aaaPkData = reader.ReadSmartStreamToBytes(smartStreamLen);
            BinaryXmlDocument aaaPkDoc = new BinaryXmlDocument();
            aaaPkDoc.Read(Encoding.Unicode, aaaPkData);
            BinaryXmlTag aaaPkXml = aaaPkDoc.RootTag;
            BinaryXmlTag? soundTag = aaaPkXml["PackFolder"].FirstOrDefault(x => x.GetAttribute("name") == "sound");
            if(soundTag is null)
                aaaPkXml.Add(soundTag = new BinaryXmlTag("PackFolder")
                    .SetAttributeContinue("name", "sound")
                );
            BinaryXmlTag? bgmTag = soundTag["PackFolder"].FirstOrDefault(x => x.GetAttribute("name") == "bgm");
            if(bgmTag is null)
                soundTag.Add(bgmTag = new BinaryXmlTag("PackFolder")
                    .SetAttributeContinue("name", "bgm")
                );
            BinaryXmlTag? soundKoreaTag = bgmTag["RhoFolder"].FirstOrDefault(x => x.GetAttribute("name") == "korea");
            // <RhoFolder name="korea" fileName="sound_bgm_korea.rho" key="3175846090" dataHash="650954873" mediaSize="5170944"/>
            if (soundKoreaTag is null)
                bgmTag.Add(soundKoreaTag = new BinaryXmlTag("RhoFolder")
                    .SetAttributeContinue("name", "korea")
                    .SetAttributeContinue("fileName", "sound_bgm_korea.rho")
                    .SetAttributeContinue("key", "3175846090")
                    .SetAttributeContinue("dataHash", "650954873")
                    .SetAttributeContinue("mediaSize", "5170944")
                );
            BinaryXmlTag? themeTag = aaaPkXml["PackFolder"].FirstOrDefault(x => x.GetAttribute("name") == "theme");
            if(themeTag is null)
                aaaPkXml.Add(themeTag = new BinaryXmlTag("PackFolder")
                    .SetAttributeContinue("name", "theme")
                );
            BinaryXmlTag? themeKoreaTag = themeTag["RhoFolder"].FirstOrDefault(x => x.GetAttribute("name") == "korea");
            // <RhoFolder name="korea" fileName="theme_korea.rho" key="2411040543" dataHash="156882519" mediaSize="9880832"/>
            if(themeKoreaTag is null)
                themeTag.Add(themeKoreaTag = new BinaryXmlTag("RhoFolder")
                    .SetAttributeContinue("name", "korea")
                    .SetAttributeContinue("fileName", "theme_korea.rho")
                    .SetAttributeContinue("key", "2411040543")
                    .SetAttributeContinue("dataHash", "156882519")
                    .SetAttributeContinue("mediaSize", "9880832")
                );

            aaaPkXml["PackFolder"]
                .FirstOrDefault(x => x.GetAttribute("name") == "track")
                ?["RhoFolder"].FirstOrDefault(x => x.GetAttribute("name") == "common")
                ?.SetAttributeContinue("dataHash", $"{trackCommonHash}")
                ?.SetAttributeContinue("mediaSize", $"{trackCommonLength}");
            
            aaaPkXml["RhoFolder"]
                .FirstOrDefault(x => x.GetAttribute("name") == "trackThumb")
                ?.SetAttributeContinue("dataHash", $"{trackThumbHash}")
                ?.SetAttributeContinue("mediaSize", $"{trackThumbLength}");
            
            aaaPkXml["PackFolder"]
                .FirstOrDefault(x => x.GetAttribute("name") == "dialog2")
                ?["RhoFolder"].FirstOrDefault(x => x.GetAttribute("name") == "selectTrackEx")
                ?.SetAttributeContinue("dataHash", $"{dialog2Hash}")
                ?.SetAttributeContinue("mediaSize", $"{dialog2Size}");
            
            using (FileStream outStream = new FileStream(Path.Combine(patchDestFolder, "aaa.pk"), FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(outStream);
                byte[] aaaPkOut = aaaPkXml.ToBinary(Encoding.Unicode);
                writer.WriteAsSmartStreamData(aaaPkOut, SmartStreamMode.CompressedEncrypted, true, 0x36699336);
            }
            System.IO.File.Copy(
                Path.Combine(copyFolder, "sound_bgm_korea.rho"),
                Path.Combine(patchDestFolder, "sound_bgm_korea.rho")
            );
            System.IO.File.Copy(
                Path.Combine(copyFolder, "theme_korea.rho"),
                Path.Combine(patchDestFolder, "theme_korea.rho")
            );
        }
        
        return new CommandExecuteResult(ResultType.Success, "");
    }

    private void createPatchedFolder()
    {
        if (!Directory.Exists(_patchPatch))
            Directory.CreateDirectory(_patchPatch);
    }

    private void mountToRhoFolder<TFolder, TFile>(TFolder mountToFolder, DirectoryInfo mountFrom) 
        where TFile: IModifiableRhoFile, new()
        where TFolder: IModifiableRhoFolder, new()
    {
        Queue<(IModifiableRhoFolder, DirectoryInfo)> queue = new Queue<(IModifiableRhoFolder, DirectoryInfo)>();
        queue.Enqueue((mountToFolder, mountFrom));
        while (queue.Count > 0)
        {
            var curObj = queue.Dequeue();
            foreach (var childFolder in curObj.Item2.GetDirectories())
            {
                IModifiableRhoFolder? mountFolder = curObj.Item1.GetFolder(childFolder.Name);
                if (mountFolder is null)
                {
                    mountFolder = new TFolder();
                    mountFolder.Name = childFolder.Name;
                    curObj.Item1.AddFolder(mountFolder);
                }
                queue.Enqueue((mountFolder, childFolder));
            }

            foreach (var childFile in curObj.Item2.GetFiles())
            {
                IModifiableRhoFile? replaceFile = curObj.Item1.GetFile(childFile.Name);
                if (replaceFile is null)
                {
                    replaceFile = new TFile();
                    replaceFile.Name = childFile.Name;
                    replaceFile.DataSource = new FileDataSource(childFile.FullName);
                    curObj.Item1.AddFile(replaceFile);
                }
            }
        }
    }
}