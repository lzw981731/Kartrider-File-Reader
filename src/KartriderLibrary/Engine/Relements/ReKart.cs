using System.Numerics;
using System.Text;
using eP.Text;
using KartCity.Common.IO;
using KartLibrary.IO;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class ReKart: ReToon
    {
        private Vector3 _unknownVec3_2;
        private Vector3 _unknownVec3_3;
        private Vector4 _unknownVec4_4;

        public override string ClassName => "ReKart";
        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            _unknownVec3_2 = reader.ReadVector3();
            _unknownVec3_3 = reader.ReadVector3();
            _unknownVec4_4 = reader.ReadVector4();
        }

        protected override void ConstructOtherInfo(StringBuilder stringBuilder, int indentLevel)
        {
            base.ConstructOtherInfo(stringBuilder, indentLevel);
            string indendStr = "".PadLeft(indentLevel << 2, ' ');
            stringBuilder.AppendLine($"{indendStr}    <ReKartProperties>");
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownInt_1", ToonUnknownInt1);

            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownVec3_2", _unknownVec3_2);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownVec3_3", _unknownVec3_3);
            stringBuilder.ConstructPropertyString(indentLevel + 2, "_unknownVec4_4", _unknownVec4_4);
            stringBuilder.AppendLine($"{indendStr}    </ReKartProperties>");
        }
    }
}
