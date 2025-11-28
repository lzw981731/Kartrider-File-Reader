using System.Text;
using KartCity.Common.Xml;

namespace KartCityStudio.Common.Converter.Implements;

public class BmlConverter: IFileConverter
{
    public string SourceExtension => ".bml";
    public string DestinationExtension => ".xml";
    public byte[] ConvertFile(byte[] orgFile)
    {
        using var inStream = new MemoryStream(orgFile);
        using var outStream = new MemoryStream();
        
        BinaryReader reader = new BinaryReader(inStream);
        BinaryWriter writer = new BinaryWriter(outStream);
        
        BinaryXmlTag tag = reader.ReadBinaryXmlTag(Encoding.Unicode);
        string xmlStr = tag.ToString();
        writer.Write(Encoding.UTF8.GetBytes(xmlStr));
        return outStream.ToArray();
    }
}