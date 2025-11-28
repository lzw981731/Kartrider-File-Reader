using System.Text;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Client;

[KartObjectImplement]
public class AccountDataSp: AccountData
{
    public override string ClassName => "AccountDataSp";

    public byte[] SpData { get; set; } = [];
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        int byteArrLen = reader.ReadInt32();
        SpData = reader.ReadBytes(byteArrLen);
    }
}