using KartCity.Common.IO;

namespace RayCityLibrary.IO
{
    public class NamedRayCityObject: RayCityObject
    {
        public string Name { get; set; } = "";
        protected NamedRayCityObject() 
        {
            
        }
        public override void DecodeObject(BinaryReader reader, RayCityObjectBuffer? buffer)
        {
            Name = reader.ReadKRString();
        }
        public override void EncodeObject(BinaryWriter writer, RayCityObjectBuffer? buffer)
        {
            writer.WriteKRString(Name);
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}: {Name}";
        }
    }
}
