using System.Text;
using KartCity.Common.IO;

namespace KartCity.Common.Xml;

public static class BinaryXmlExtension
{
    public static BinaryXmlTag ReadBinaryXmlTag(this BinaryReader br, Encoding encoding)
    {
        BinaryXmlTag tag = new BinaryXmlTag();
        tag.Name = br.ReadKRString();
        //Text
        tag.Text = br.ReadKRString();
        //Attributes
        int attCount = br.ReadInt32();
        for (int i = 0; i < attCount; i++)
            tag.SetAttribute(br.ReadKRString(), br.ReadKRString());
        //SubTags
        int subCount = br.ReadInt32();
        for (int i = 0; i < subCount; i++)
            tag.Children.Add(br.ReadBinaryXmlTag(encoding));
        return tag;
    }
}