using System.Numerics;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Game.Engine.Track;

[KartObjectImplement]
public class ToMinimap: TrackObject
{
    public override string ClassName => "ToMinimap";

    public Vector3 Unknown2 { get; set; }
    
    public Vector3 Unknown3 { get; set; }
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        Unknown2 = reader.ReadVector3();
        Unknown3 = reader.ReadVector3();
    }
}