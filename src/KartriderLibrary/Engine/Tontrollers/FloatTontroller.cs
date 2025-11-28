using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class FloatTontroller : Tontroller
    {
        public override string ClassName => "FloatTontroller";

        public IFloatKeyData? KeyData { get; set; }

        public FloatTontroller()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            KeyData = reader.ReadField(buffer, (reader, buffer) =>
            {
                FloatKeyDataType keyDataType = (FloatKeyDataType)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(FloatKeyDataType), keyDataType))
                    throw new Exception($"Unknown FloatKeyData type: {(int)keyDataType}");
                int length = reader.ReadInt32();
                FloatKeyDataFactory factory = new FloatKeyDataFactory();
                IFloatKeyData keyData = factory.CreateFloatKeyframeData(keyDataType);
                keyData.DecodeObject(reader, length);
                return keyData;
            });
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

        public float GetValue(float time)
        {
            if (time > base.endTime && _loopCount == 0)
            {
                if (startTime == endTime)
                    time = startTime;
                else
                    time = ((time - endTime) % (endTime - startTime)) + startTime;
            }
            return KeyData?.GetValue(time) ?? 0;
        }
    }
}
