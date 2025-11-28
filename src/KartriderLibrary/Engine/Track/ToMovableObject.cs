using System.Numerics;
using KartCity.Common.IO;
using KartLibrary.Engine.Relements;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class ToMovableObject: TrackObject
{
    public override string ClassName => "ToMovableObject";
    
    public Vector3[] Unknown2 { get; set; } = [];
    
    public Vector3 Unknown3 { get; set; }
    
    public Vector3 Unknown4 { get; set; }
    public Relement Unknown5 { get; set; }
    public int Unknown6 { get; set; }

    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        Unknown2 = new Vector3[3];
        for (int i = 0; i < 3; i++)
            Unknown2[i] = reader.ReadVector3();
        Unknown3 = reader.ReadVector3();
        Unknown4 = reader.ReadVector3();
        Unknown5 = reader.ReadKartObject<Relement>(buffer);
        Unknown6 = reader.ReadInt32();
    }
}