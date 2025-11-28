using System.Text;
using eP.Text;
using KartLibrary.IO;
using Veldrid;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class ReTriList : ReTriCommon
    {
        public override string ClassName => "ReTriList";

        private int _unknownInt_1;
        private VertexData _vertexData;
        
        public override VertexData Vertex => _vertexData;

        public ReTriList() : base(PrimitiveTopology.TriangleList)
        {
            
        }
        
        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            _unknownInt_1 = reader.ReadInt32();
            _vertexData = reader.ReadField(buffer, VertexData.Deserialize);
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        protected override void ConstructOtherInfo(StringBuilder stringBuilder, int indentLevel)
        {
            base.ConstructOtherInfo(stringBuilder, indentLevel);
            string indendStr = "".PadLeft(indentLevel << 2, ' ');
            stringBuilder.AppendLine($"{indendStr}<ReTriStripProperties>");
            stringBuilder.ConstructPropertyString(indentLevel + 1, "_unknownInt_1", _unknownInt_1);
            stringBuilder.AppendLine($"{indendStr}</ReTriStripProperties>");
            stringBuilder.ConstructPropertyString(indentLevel, "TriStrip", Vertex);
        }
    }
}
