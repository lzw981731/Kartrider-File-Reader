using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.IO;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class MorphTontroller : Tontroller
    {
        public override string ClassName => "MorphTontroller";

        public MorphData[] MorphDataArray = [];
        
        public MorphTontroller()
        {
            
        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);

            MorphDataArray = reader.ReadField(buffer, (reader, buffer1) =>
            {
                int count = reader.ReadInt32();
                return Enumerable.Range(0, count).Select(x => MorphData.Decode(reader, buffer1)).ToArray();
            });
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    }

    public class MorphData
    {
        public Vector3[]? Unknown1 { get; set; }
        public Vector3[]? Unknown2 { get; set; }
        public float[]? Unknown3 { get; set; }
        public Vector2[]? Unknown4 { get; set; }
        public IFloatKeyData FloatKeyData { get; set; }

        public static MorphData Decode(BinaryReader reader, KartObjectBuffer? buffer)
        {
            MorphData output = new MorphData();
            short count = reader.ReadInt16();
            if (reader.ReadBoolean())
            {
                output.Unknown1 = Enumerable.Range(0, count).Select(x => reader.ReadVector3()).ToArray();
            }
            if (reader.ReadBoolean())
            {
                output.Unknown2 = Enumerable.Range(0, count).Select(x => reader.ReadVector3()).ToArray();
            }
            if (reader.ReadBoolean())
            {
                output.Unknown3 = Enumerable.Range(0, count).Select(x => reader.ReadSingle()).ToArray();
            }
            if (reader.ReadBoolean())
            {
                output.Unknown4 = Enumerable.Range(0, count).Select(x => reader.ReadVector2()).ToArray();
            }
            
            output.FloatKeyData = reader.ReadField(buffer, (reader, buffer1) =>
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

            return output;
        }
    }
}
