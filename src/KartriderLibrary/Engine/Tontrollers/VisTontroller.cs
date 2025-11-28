using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class VisTontroller : Tontroller
    {
        public override string ClassName => "VisTontroller";

        public IBoolKeyData? BoolKeyData; 

        public VisTontroller()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            BoolKeyData = reader.ReadField(buffer, (reader, buffer) =>
            {
                BoolKeyDataType keyDataType = (BoolKeyDataType)reader.ReadInt32();
                int count = reader.ReadInt32();
                IBoolKeyData boolKeyData = new BoolKeyDataFactory().CreateBoolKeyframeData(keyDataType);
                boolKeyData.DecodeObject(reader, count);
                return boolKeyData;
            });
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsVisible(float time)
        {
            return BoolKeyData?.GetValue(time) ?? true;
        }
    }
}
