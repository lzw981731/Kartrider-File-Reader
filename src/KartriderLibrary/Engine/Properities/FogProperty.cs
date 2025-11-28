using System.Diagnostics;
using System.Drawing;
using KartCity.Common.Engine.Enums;
using KartLibrary.IO;

namespace KartLibrary.Engine.Properities;

[KartObjectImplement]
public class FogProperty: KartObject
{
    public FogPropertyMode FogPropertyMode { get; set; }
    
    public D3DFogMode FogMode { get; set; }
    
    public Color FogColor { get; set; }
    
    public float FogStart { get; set; }
    
    public float FogEnd { get; set; }
    
    public float FogDensity { get; set; }
    
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        FogPropertyMode = (FogPropertyMode)reader.ReadInt32(); // Default: 0, fogMode, if == 1, enable, disable else.
        FogMode = (D3DFogMode) reader.ReadInt32(); // Default: 1 mode, FogVertexMode or FogTableMode
        FogColor = Color.FromArgb(reader.ReadInt32()); // Default: 0 (*) r,g,b, FogColor
        FogStart = reader.ReadSingle(); // Default: 0, FogStart
        FogEnd = reader.ReadSingle(); // Default: 1.0f, FogEnd
        FogDensity = reader.ReadSingle(); // Default: 1.0f, FogDensity
    }
}