using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class IntTontroller : Tontroller
    {
        public override string ClassName => "IntTontroller";
        
        public IIntKeyData? KeyData { get; set; }

        public IntTontroller() 
        {
            
        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            KeyData = reader.ReadField(buffer, (reader, buffer) =>
            {
                IntKeyDataType keyDataType = (IntKeyDataType)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(IntKeyDataType), keyDataType))
                    throw new Exception($"Unknown IntKeyDataType: {(int)keyDataType}");
                int count = reader.ReadInt32();
                IIntKeyData keyData = new IntKeyDataFactory().CreateIntKeyframeData(keyDataType);
                keyData.DecodeObject(reader, count);
                return keyData;
            });
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    }
}
