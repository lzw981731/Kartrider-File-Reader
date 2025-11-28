using System.Text;
using eP.Text;
using KartLibrary.IO;
using Vortice.Direct3D11;

namespace KartLibrary.Engine.Properities
{
    [KartObjectImplement]
    public class BackFaceProperty : KartObject
    {
        private CullMode _cullMode;

        public CullMode CullMode => _cullMode;

        public override string ClassName => "BackFaceProperty";

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            _cullMode = (CullMode)reader.ReadInt32();
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<BackFaceProperty>");
            stringBuilder.ConstructPropertyString(1, "CullMode", CullMode);
            stringBuilder.Append("</BackFaceProperty>");
            return stringBuilder.ToString();
        }
    }
}
