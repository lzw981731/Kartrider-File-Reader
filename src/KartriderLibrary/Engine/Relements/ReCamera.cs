using System.Text;
using eP.Text;
using KartLibrary.IO;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class ReCamera : Relement
    {
        public override string ClassName => "ReCamera";
        private byte u1;
        
        private float u2; // Default: 90.0f, fov
        private KartObject? uObj2;
        
        private float u3; // Default: 1.0f, near
        private KartObject? uObj3;
        
        private float u4; // Default: 512.0f, far
        private KartObject? uObj4;

        public ReCamera()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            u1 = reader.ReadByte();
            u2 = reader.ReadSingle();
            if (reader.ReadByte() == 1)
            {
                uObj2 = reader.ReadKartObject(buffer);
            }
            u3 = reader.ReadSingle();
            if (reader.ReadByte() == 1)
            {
                uObj3 = reader.ReadKartObject(buffer);
            }
            u4 = reader.ReadSingle();
            if (reader.ReadByte() == 1)
            {
                uObj4 = reader.ReadKartObject(buffer);
            }
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        protected override void ConstructOtherInfo(StringBuilder stringBuilder, int indentLevel)
        {
            base.ConstructOtherInfo(stringBuilder, indentLevel);
            string indendStr = "".PadLeft(indentLevel << 2, ' ');
            stringBuilder.AppendLine($"{indendStr}<ReCameraProperties>");
            stringBuilder.ConstructPropertyString(indentLevel + 1, "u1", u1);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "u2", u2);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "u3", u3);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "u4", u4);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "uObj2", uObj2);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "uObj3", uObj3);
            stringBuilder.ConstructPropertyString(indentLevel + 1, "uObj4", uObj4);
            stringBuilder.AppendLine($"{indendStr}</ReCameraProperties>");
        }
    }
}
