using System.Diagnostics;
using KartLibrary.IO;
using Color = System.Drawing.Color;

namespace KartLibrary.Engine.Properities;

[KartObjectImplement]
public class ToonProperty: KartObject
{
    public Color UnknownColor1; 
    public Color UnknownColor2; 
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        byte u1 = reader.ReadByte();
        byte u2 = reader.ReadByte();
        int u3 = reader.ReadInt32();
        int u4 = reader.ReadInt32();
        int u5 = reader.ReadInt32();
        float u8 = reader.ReadSingle();
        int u9 = reader.ReadInt32();
        int u10 = reader.ReadInt32();
        UnknownColor1 = Color.FromArgb(reader.ReadInt32());
        UnknownColor2 = Color.FromArgb(reader.ReadInt32());
        // int u6 = reader.ReadInt32();
        // int u7 = reader.ReadInt32();
        int u11 = reader.ReadInt32();
        Debug.Print($"Toon: {UnknownColor1} {UnknownColor2}");
    }
}