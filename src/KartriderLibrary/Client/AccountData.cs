using System.Text;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Client;

[KartObjectImplement]
public class AccountData: KartObject
{
    public override string ClassName => "AccountData";
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        byte u1 = reader.ReadByte();
    }
}