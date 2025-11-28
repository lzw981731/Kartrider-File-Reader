using System.Diagnostics;
using System.Text.RegularExpressions;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.FileType;
using KartCity.Common.Xml;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.Xml;

namespace KartLibrary.Game.Kart;

public class KartManager
{
    private Dictionary<CountryCode, Dictionary<string, KartBodyParam>> _kartBodyParams
        = new Dictionary<CountryCode, Dictionary<string, KartBodyParam>>();

    private Dictionary<KartSpeedLevel, KartBodyParam> _kartLevelParams = new Dictionary<KartSpeedLevel, KartBodyParam>();
    
    public KartManager()
    {
        
    }

    public void Initialize(KartStorageSystem storageSystem)
    {
        KartStorageFolder? kartOldRootFolder = storageSystem.GetFolder("kart");
        KartStorageFolder? kartNewRootFolder = storageSystem.GetFolder("kart_");
        var kartRootFolders = new []{ kartOldRootFolder, kartNewRootFolder };
        Regex paramFileNamePattern = new Regex(@"^param(?:@(\w{2})){0,1}\.(?:xml|bml)$");
        Regex levelPattern = new Regex(@"^level(\d+)\.(?:xml|bml|kml)$");
        foreach (KartStorageFolder? kartRootFolder in kartRootFolders)
        {
            if(kartRootFolder is null)
                continue;
            foreach (var kartFolder in kartRootFolder.Folders)
            {
                string kartName = kartFolder.Name;
                bool hasParams = false;
                if (kartName == "level")
                {
                    foreach (var kartFile in kartFolder.Files)
                    {
                        Match match = levelPattern.Match(kartFile.Name);
                        if (match.Success)
                        {
                            int levelId =  Convert.ToInt32(match.Groups[1].Value);
                            BinaryXmlTag paramXmlTag = kartFile.ReadXml();
                            KartBodyParam? kartBodyParam = KartBodyParam.ReadFromXml(paramXmlTag);
                            if (kartBodyParam is not null)
                                _kartLevelParams.TryAdd((KartSpeedLevel)levelId, kartBodyParam);
                        }
                    }
                }
                else
                {
                    foreach (var kartFile in kartFolder.Files)
                    {
                        Match match = paramFileNamePattern.Match(kartFile.Name);
                        if (match.Success)
                        {
                            hasParams = true;
                            string region = match.Groups[1].Value;
                            CountryCode clientCountry;
                            if (!Enum.TryParse(region.ToUpper(), out clientCountry))
                                clientCountry = CountryCode.None;
                            _kartBodyParams.TryAdd(clientCountry, new Dictionary<string, KartBodyParam>());
                            BinaryXmlTag paramXmlTag = kartFile.ReadXml();
                            KartBodyParam? kartBodyParam = KartBodyParam.ReadFromXml(paramXmlTag);
                            if (kartBodyParam is not null && !_kartBodyParams[clientCountry].TryAdd(kartName, kartBodyParam))
                                _kartBodyParams[clientCountry][kartName] = kartBodyParam;
                        }
                    }
                }

                if (!hasParams)
                {
                    Debug.WriteLine($"{kartFolder.FullName}");
                }
            }
        }
    }

    public KartBodyParam? GetKartBodyParam(string kartName, CountryCode clientCountry)
    {
        if (!_kartBodyParams.TryGetValue(CountryCode.None, out var globalParams)
            || !globalParams.TryGetValue(kartName, out var globalParam))
            return null;
        KartBodyParam output = globalParam.Clone();
        if (_kartBodyParams.TryGetValue(clientCountry, out var regionParams)
            && regionParams.TryGetValue(kartName, out var regionParam))
        {
            foreach (var paramName in regionParam.GetDefinedParamNames)
                output[paramName] = regionParam[paramName];
        }
        
        return output;
    }
    
    public KartBodyParam? GetKartLevelParam(int level)
    {
        return _kartLevelParams.TryGetValue((KartSpeedLevel)level, out var output) ? output : null;
    }
    
    public KartBodyParam? GetKartLevelParam(KartSpeedLevel level)
    {
        return _kartLevelParams.TryGetValue(level, out var output) ? output : null;
    }
}