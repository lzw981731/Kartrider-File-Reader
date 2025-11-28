using System.Diagnostics;
using KartLibrary.IO;

namespace KartLibrary.Engine.Properities
{
    // Material Property
    [KartObjectImplement]
    public class MtlProperty : KartObject 
    {
        public int u1;
        public int u2;
        public int u3;
        public int u4;
        public int u5; // float
        public byte u6;
        public int u7;
        public KartObject u8;
        public KartObject u9;
        public KartObject u10;
        public KartObject u11;


        public override string ClassName => "MtlProperty";

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            u1 = reader.ReadInt32();
            u2 = reader.ReadInt32();
            u3 = reader.ReadInt32();
            u4 = reader.ReadInt32();
            u5 = reader.ReadInt32(); // float
            u6 = reader.ReadByte();
            u7 = reader.ReadInt32();
            if(reader.ReadByte() != 0)
            {
                u8 = reader.ReadKartObject(buffer);
            }
            if (reader.ReadByte() != 0)
            {
                u9 = reader.ReadKartObject(buffer);
            }
            if (reader.ReadByte() != 0)
            {
                u10 = reader.ReadKartObject(buffer);
            }
            if (reader.ReadByte() != 0)
            {
                u11 = reader.ReadKartObject(buffer);
            }
            Debug.Print($"Mtl");
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    }
}
