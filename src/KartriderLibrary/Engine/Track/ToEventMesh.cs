using System.Numerics;
using System.Text;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class ToEventMesh: TrackObject
{
    public override string ClassName => "ToEventMesh";

    public string Unknown2 { get; set; } = "";
    
    public Vector3[] Unknown3 { get; set; } = [];
    
    public string Unknown4 { get; set; } = "";
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        Unknown2 = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt16() << 1));
        Unknown3 = new Vector3[reader.ReadInt16()];
        for (int i = 0; i < Unknown3.Length; i++)
            Unknown3[i] = reader.ReadVector3();
        Unknown4 = reader.ReadKRString();
        
    }
}