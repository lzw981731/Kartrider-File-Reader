using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartLibrary.Game.Kart;

public class KartBodyParam
{
    private Dictionary<string, string> _paramValues = new Dictionary<string, string>();
    
    public ICollection<string> GetDefinedParamNames => _paramValues.Keys;
    
    public ICollection<string> GetDefinedParamValues => _paramValues.Values;
    
    public string? this[string paramName]
    {
        get
        {
            paramName = paramName.ToLower();
            _paramValues.TryGetValue(paramName, out string? outValue);
            return outValue;
        }
        set
        {
            paramName = paramName.ToLower();
            if (value is not null && !_paramValues.TryAdd(paramName, value))
                _paramValues[paramName] = value;
        }
    }

    public string? this[KartBodyParamType paramType] => this[paramType.ToString()];

    public static KartBodyParam? ReadFromXml(BinaryXmlTag paramXmlTag)
    {
        if (paramXmlTag.Name.ToLower() != "bodyparam" && paramXmlTag.Name.ToLower() != "dynamics")
            return null;
        KartBodyParam kartBodyParam = new KartBodyParam();
        foreach (var attr in paramXmlTag.Attributes)
        {
            kartBodyParam[attr.Key] = attr.Value;
        }

        return kartBodyParam;
    }

    public KartBodyParam Clone()
    {
        KartBodyParam output = new KartBodyParam();
        foreach(var keyPair in _paramValues)
            output._paramValues.Add(keyPair.Key, keyPair.Value);
        return output;
    }

    public float? GetParamAsSingle(string attributeName)
    {
        if (_paramValues.TryGetValue(attributeName.ToLower(), out var paramStrVal) &&
            float.TryParse(paramStrVal ?? "0", out var paramVal))
            return paramVal;
        return null;
    }
    
    public int? GetParamAsInt32(string attributeName)
    {
        if (_paramValues.TryGetValue(attributeName.ToLower(), out var paramStrVal) &&
            int.TryParse(paramStrVal ?? "0", out var paramVal))
            return paramVal;
        return null;
    }
    
    public bool? GetParamAsBoolean(string attributeName)
    {
        if (_paramValues.TryGetValue(attributeName.ToLower(), out var paramStrVal) &&
            bool.TryParse(paramStrVal ?? "0", out var paramVal))
            return paramVal;
        return null;
    }
}