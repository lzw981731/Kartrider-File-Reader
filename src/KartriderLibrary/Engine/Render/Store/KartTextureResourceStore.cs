using System.Collections.Concurrent;
using KartCity.Common.FileType;
using KartLibrary.File;
using Veldrid;

namespace KartLibrary.Game.Engine.Render.Store;

public class KartTextureResourceStore: TextureResourceStore
{
    private KartStorageSystem _kartStorageSystem;

    private ConcurrentDictionary<string, (string, bool)> _generalResourceMap = [];
    private ConcurrentDictionary<string, (string, bool)> _trackResourceMap = [];
    private ConcurrentDictionary<string, (string, bool)> _themeResourceMap = [];
    private ConcurrentDictionary<string, (string, bool)> _pplResourceMap = [];
    
    public KartTextureResourceStore(KartStorageSystem storageSystem, GraphicsDevice graphicsDevice): base(graphicsDevice)
    {
        _kartStorageSystem = storageSystem;
    }
    
    protected override Stream? CreateTextureStream(string resourceName, out bool isDDs)
    {
        isDDs = false;
        if (_trackResourceMap.TryGetValue(resourceName, out var fileNamePair)
            || _themeResourceMap.TryGetValue(resourceName, out fileNamePair)
            || _pplResourceMap.TryGetValue(resourceName, out fileNamePair)
            || _generalResourceMap.TryGetValue(resourceName, out fileNamePair))
        {
            var textureFile = _kartStorageSystem.GetFile(fileNamePair.Item1);
            isDDs = fileNamePair.Item2;
            
            return textureFile?.CreateStream();
        }
        else
        {
            return null;
        }
    }

    public void LoadGeneralResource(string path)
    {
        var folder = _kartStorageSystem.GetFolder(path);
        foreach (KartStorageFile textureFile in folder?.Files ?? [])
        {
            string textureName = textureFile.NameWithoutExt;
            if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
                textureFile.Name.EndsWith(".tga"))
            {
                bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
                _generalResourceMap.GetOrAdd(textureName, (textureFile.FullName, isDDs));
            }
        }
    }
    
    public void LoadTrackResource(string track)
    {
        var trackList = _kartStorageSystem.GetFile("track/common/track@zz.bml");
        if (trackList is null)
            throw new Exception("Can't found track/common/track@zz.bml");

        var trackListXml = trackList.ReadXml();
        var trackInfo = trackListXml.Children.FirstOrDefault(x => string.Compare(track, x.GetAttribute("id")) == 0);
        if (trackInfo is null)
            throw new Exception($"Can't found track: {track}");

        var theme = track.Split("_").First();
        var useFolder = (((string?)trackInfo.GetAttribute("texTheme") ?? "")).Split("|");
        string[] themeNames = [theme, "common", ..useFolder];

        var trackFolder = _kartStorageSystem.GetFolder($"track/{track}") ??
                          _kartStorageSystem.GetFolder($"track_/{track}");
        if (trackFolder is null)
            throw new Exception($"Can't found track folder: {track}");

        foreach (KartStorageFile textureFile in trackFolder?.Files ?? [])
        {
            string textureName = textureFile.NameWithoutExt;
            if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
                textureFile.Name.EndsWith(".tga"))
            {
                bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
                _trackResourceMap.GetOrAdd(textureName, (textureFile.FullName, isDDs));
            }
        }

        foreach (var themeName in themeNames)
        {
            KartStorageFolder? themeFolder = _kartStorageSystem.GetFolder($"theme/{themeName}/texture");
            foreach (KartStorageFile textureFile in themeFolder?.Files ?? [])
            {
                if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
                    textureFile.Name.EndsWith(".tga"))
                {
                    string textureName = textureFile.NameWithoutExt;
                    bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
                    _themeResourceMap.GetOrAdd(textureName, (textureFile.FullName, isDDs));
                }
            }
        }

        string[] pplNames = ["fengshen2"];
        foreach (var pplName in pplNames)
        {
            KartStorageFolder? themeFolder = _kartStorageSystem.GetFolder($"zeta/cn/ppl/ingame/{pplName}/village");
            foreach (KartStorageFile textureFile in themeFolder?.Files ?? [])
            {
                string textureName = textureFile.NameWithoutExt;
                bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
                _pplResourceMap.GetOrAdd(textureName, (textureFile.FullName, isDDs));
            }
        }
    }
}