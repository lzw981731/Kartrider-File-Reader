using System.Text;
using eP.Text;
using KartCity.Common.Engine.Enums;
using KartLibrary.Engine.Enums;
using KartLibrary.IO;
using Vortice.Direct3D11;

namespace KartLibrary.Engine.Properities
{
    [KartObjectImplement]
    public class AlphaProperty : KartObject
    {
        public override string ClassName => "AlphaProperty";
        public bool UseBlendTest { get; set; }
        public D3DBlendFactor SourceColorFactor { get; set; } //Source Color Factor (D3DBLEND)
        public D3DBlendFactor DestinationColorFactor { get; set; } // Desc Color Factor (D3DBLEND)
        public bool UseAlphaTest { get; set; }
        public ComparisonFunction AlphaFunction { get; set; }
        public byte AlphaTestRef { get; set; }

        public AlphaProperty()
        {
            
        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            UseBlendTest = (reader.ReadByte() & 1) != 0;
            SourceColorFactor = (D3DBlendFactor)reader.ReadInt32();
            DestinationColorFactor = (D3DBlendFactor)reader.ReadInt32();
            UseAlphaTest = reader.ReadByte() != 0;
            AlphaFunction = (ComparisonFunction)reader.ReadInt32();
            AlphaTestRef = reader.ReadByte();
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<AlphaProperty>");
            stringBuilder.ConstructPropertyString(1, "UseBlendTest", UseBlendTest);
            stringBuilder.ConstructPropertyString(1, "SourceColorFactor", SourceColorFactor);
            stringBuilder.ConstructPropertyString(1, "DestinationColorFactor", DestinationColorFactor);
            stringBuilder.ConstructPropertyString(1, "UseAlphaTest", UseAlphaTest);
            stringBuilder.ConstructPropertyString(1, "AlphaFunction", AlphaFunction);
            stringBuilder.ConstructPropertyString(1, "AlphaTestRef", AlphaTestRef);
            stringBuilder.AppendLine("</AlphaProperty>");
            return stringBuilder.ToString();
        }
    }
}
