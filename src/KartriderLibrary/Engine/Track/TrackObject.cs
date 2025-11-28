using System.Text;
using KartCity.Common.Xml;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class TrackObject: NamedKartObject
{
    public override string ClassName => "TrackObject";

    public BinaryXmlTag? Unknown1 { get; set; }
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        if (reader.ReadByte() != 0)
            Unknown1 = reader.ReadField(buffer, (x, _) => x.ReadBinaryXmlTag(Encoding.Unicode));
    }
}