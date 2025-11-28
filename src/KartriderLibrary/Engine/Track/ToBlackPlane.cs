using System.Numerics;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class ToBlackPlane: TrackObject
{
    public override string ClassName => "ToBlackPlane";

    public Vector3[] Unknown2 { get; set; } = [];
    
    public Vector3 Unknown3 { get; set; }
    public byte Unknown4 { get; set; }
    public byte Unknown5 { get; set; }
    public byte Unknown6 { get; set; }
    public byte Unknown7 { get; set; }
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        Unknown2 = new Vector3[4];
        for (int i = 0; i < 4; i++)
            Unknown2[i] = reader.ReadVector3();
        Unknown3 = reader.ReadVector3();
        Unknown4 = reader.ReadByte();
        Unknown5 = reader.ReadByte();
        Unknown6 = reader.ReadByte();
        Unknown7 = reader.ReadByte();
    }
}