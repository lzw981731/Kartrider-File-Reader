using System.Diagnostics;
using System.Text;
using eP.Text;
using KartLibrary.IO;
using Vortice.Direct3D11;

namespace KartLibrary.Engine.Properities
{
    [KartObjectImplement]
    public class ZBufProperty : KartObject
    {
        public override string ClassName => "ZBufProperty";

        public ComparisonFunction DepthFunction { get; set; }
        
        public bool EnableDepthWrite { get; set; }

        public ZBufProperty()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            DepthFunction = (ComparisonFunction) reader.ReadInt32();
            EnableDepthWrite = reader.ReadBoolean();
            // Debug.Print($"ZBuf: {this}");
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {

        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // stringBuilder.AppendLine($"<u1>{u1}({u1:x8})</u1>");
            // stringBuilder.AppendLine($"<u2>{u2}({u2:x2})</u2>");
            return stringBuilder.ToString();
        }
    }
}
