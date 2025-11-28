using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using KartCity.Common.Xml;

namespace KartCity.Common.FileType;

public static class RhoFileExtension
{
    public static BinaryXmlTag ReadXml(this IRhoFile rhoFile, bool? isBinaryFormat = null)
    {
        if (isBinaryFormat is null)
        {
            string extension = rhoFile.Name.Length >= 4 ? rhoFile.Name[^4..] : "";
            isBinaryFormat = extension.ToLower() == ".bml";
        }

        if (isBinaryFormat.Value)
        {
            BinaryXmlDocument binaryXmlDocument = new BinaryXmlDocument();
            binaryXmlDocument.Read(Encoding.Unicode, rhoFile.GetBytes());
            return binaryXmlDocument.RootTag;
        }
        else
        {   
            using (Stream rhoFileStream = rhoFile.CreateStream())
            {
                byte[] data = new byte[rhoFileStream.Length];
                rhoFileStream.Read(data);
                ushort utf16Bom = BitConverter.ToUInt16(data, 0);
                string xmlData = utf16Bom switch
                {
                    0xFEFF => Encoding.Unicode.GetString(data, 2, data.Length - 2),
                    0xFFFE => Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2),
                    _ => Encoding.UTF8.GetString(data)
                };
                xmlData = xmlData.TrimStart();
                var vaildLines = xmlData.Split('\n').Where(x => !Regex.IsMatch(x, @"^\s*<\s*\?\s*xml"));
                xmlData = "<?xml version=\"1.0\" encoding='UTF-16'?>\r\n" + string.Join('\n', vaildLines);
                byte[] tmpData = Encoding.Unicode.GetBytes(xmlData);
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreComments = true;
                using(MemoryStream tmpMs = new MemoryStream(tmpData))
                using (XmlReader xmlReader = XmlReader.Create(tmpMs, readerSettings))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(xmlReader);
                    XmlNode? rootNode = xmlDocument.ChildNodes.Cast<XmlNode>().First(x => x.NodeType == XmlNodeType.Element);
                    if (rootNode is null)
                        throw new Exception($"{rhoFile.FullName} cannot be read as xml file.");
                    return (BinaryXmlTag)(rootNode);    
                }
                
            }
        }
    }
}