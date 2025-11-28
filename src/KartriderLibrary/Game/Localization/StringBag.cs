using KartLibrary.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartLibrary.Game.Localization
{
    public class StringBag
    {
        private Dictionary<string, Dictionary<CountryCode, string>> _container = new Dictionary<string, Dictionary<CountryCode, string>>();

        public ICollection<string> Keys => _container.Keys;
        
        public StringBag()
        {

        }

        public string? GetString(CountryCode country, string key, bool defaultIfNull = true)
        {
            if (_container.ContainsKey(key))
                if (_container[key] is not null)
                    if (_container[key].ContainsKey(country))
                        return _container[key][country];
            return defaultIfNull ? $"!sb({key})" : null;
        }

        public void SetString(CountryCode country, string key, string value)
        {
            if (!_container.ContainsKey(key))
                _container.Add(key, new Dictionary<CountryCode, string>());
            if (_container[key] is null)
                _container[key] = new Dictionary<CountryCode, string>();
            if (!_container[key].ContainsKey(country))
                _container[key].Add(country, value);
            else
                _container[key][country] = value;
        }
        
        public bool LoadFromXElement(XElement xElement)
        {
            XElement? stringBagNode = xElement.DescendantsAndSelf("StringBag").FirstOrDefault();
            if (stringBagNode is null)
                return false;

            IEnumerable<XElement> keyNodes = stringBagNode.Descendants("k");
            foreach (var keyNode in keyNodes)
            {
                string? nameStr = keyNode.Attribute("n")?.Value;
                if (nameStr is not null)
                {
                    foreach (var mNode in keyNode.Descendants("m"))
                    {
                        string? countryCodeStr = mNode.Attribute("c")?.Value;
                        string valueStr = mNode.Attribute("v")?.Value ?? "(UNDEFINED)";
                        CountryCode countryCode = countryCodeStr switch
                        {
                            "kr" => CountryCode.KR,
                            "cn" => CountryCode.CN,
                            "tw" => CountryCode.TW,
                            _ => CountryCode.None
                        };
                        this.SetString(countryCode, nameStr, valueStr);
                    }
                }
            }

            return true;
        }

        public bool LoadFromBinaryXmlTag(BinaryXmlTag binaryXmlTag)
        {
            if(binaryXmlTag.Name.ToLower() != "stringbag")
                return false;

            IEnumerable<BinaryXmlTag> keyTags = binaryXmlTag.Children.Where(x => x.Name.ToLower() == "k");
            foreach (var keyTag in keyTags)
            {
                string? nameStr = keyTag.GetAttribute("n");
                if (nameStr is not null)
                {
                    foreach (var mNode in keyTag.Children.Where(x => x.Name.ToLower() == "m"))
                    {
                        string? countryCodeStr = mNode.GetAttribute("c");
                        string valueStr = mNode.GetAttribute("v") ?? "(UNDEFINED)";
                        CountryCode countryCode = countryCodeStr switch
                        {
                            "kr" => CountryCode.KR,
                            "cn" => CountryCode.CN,
                            "tw" => CountryCode.TW,
                            _ => CountryCode.None
                        };
                        this.SetString(countryCode, nameStr, valueStr);
                    }
                }
            }

            return true;
        }
    }
}
