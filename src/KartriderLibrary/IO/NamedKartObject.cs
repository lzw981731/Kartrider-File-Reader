using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.IO;

namespace KartLibrary.IO
{
    public class NamedKartObject: KartObject
    {
        public string Name { get; set; } = "";
        protected NamedKartObject() 
        {
            
        }
        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            Name = reader.ReadKRString();
        }
        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            writer.WriteKRString(Name);
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}: {Name}";
        }
    }
}
