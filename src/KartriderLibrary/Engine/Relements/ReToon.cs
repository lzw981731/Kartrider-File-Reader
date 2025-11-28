using KartLibrary.IO;

namespace KartLibrary.Engine.Relements;

public abstract class ReToon: Relement
{
    public int ToonUnknownInt1 { get; set; }
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
        ToonUnknownInt1 = reader.ReadInt32();
    }    
}