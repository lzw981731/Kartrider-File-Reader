using System.Text;
using KartCity.Common.Xml;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Client;

[KartObjectImplement]
public class AccountDataProfile: AccountData
{
    public override string ClassName => "AccountDataProfile";

    public BinaryXmlTag? ProfileData { get; set; }
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        ProfileData = reader.ReadBinaryXmlTag(Encoding.Unicode);
    }
}