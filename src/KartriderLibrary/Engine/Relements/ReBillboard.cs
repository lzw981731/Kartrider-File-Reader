using KartLibrary.IO;

namespace KartLibrary.Engine.Relements
{
    [KartObjectImplement]
    public class ReBillboard:Relement
    {
        public override string ClassName => "ReBillboard";

        public int u1;

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            u1 = reader.ReadInt32();
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }


    }
}
