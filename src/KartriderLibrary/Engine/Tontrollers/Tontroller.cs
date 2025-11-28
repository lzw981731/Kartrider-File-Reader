using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    public class Tontroller : KartObject
    {
        public override string ClassName => "Tontroller";

        private int u1;
        public int _loopCount;
        private int u3;
        private int u4;
        private int u5;
        private int u6;
        public int startTime;
        public int endTime;

        public Tontroller()
        {

        }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            u1 = reader.ReadInt32();
            _loopCount = reader.ReadInt32();
            u3 = reader.ReadInt32();
            u4 = reader.ReadInt32();
            u5 = reader.ReadInt32();
            u6 = reader.ReadInt32();
            startTime = reader.ReadInt32();
            endTime = reader.ReadInt32();
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    }
}
