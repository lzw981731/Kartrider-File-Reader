using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    [KartObjectImplement]
    public class ColorTontroller:Tontroller
    {
        public override string ClassName => "ColorTontroller";

        private IIntKeyData _colorKeyData;

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            _colorKeyData = reader.ReadField(buffer, (reader, buffer) =>
            {
                IntKeyDataType dataType = (IntKeyDataType)reader.ReadInt32();
                int count = reader.ReadInt32();
                IntKeyDataFactory factory = new IntKeyDataFactory();
                IIntKeyData intKeyData = factory.CreateIntKeyframeData(dataType);
                intKeyData.DecodeObject(reader, count);
                return intKeyData;
            });
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }

    }
}
