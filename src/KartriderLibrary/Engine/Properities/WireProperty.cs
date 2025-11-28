using System.Diagnostics;
using KartCity.Common.Engine.Enums;
using KartLibrary.IO;

namespace KartLibrary.Engine.Properities;

[KartObjectImplement]
public class WireProperty: KartObject
{
    public D3DFillMode FillMode { get; set; }
    public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
    {
        FillMode = (D3DFillMode) reader.ReadByte();
    }
}