using KartLibrary.IO;

namespace KartLibrary.Engine.Relements;

[KartObjectImplement]
public class ReCharacter: ReToon
{
    public override string ClassName => "ReCharacter";

    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        base.DecodeObject(reader, buffer);
    }
}