using System.Text;
using KartCity.Common.IO;

namespace RayCityLibrary.IO
{
    /// <summary>
    /// Represent the::Object in Raycity.
    /// It is a serializable object. 
    /// </summary>
    public abstract class RayCityObject
    {
        protected RayCityObject() 
        {
            
        }
        public virtual string ClassName => this.GetType().Name;

        public uint ClassStamp
        {
            get
            {
                byte[] classNameEnc = Encoding.UTF8.GetBytes(ClassName);
                return Adler.Adler32(0, classNameEnc, 0, classNameEnc.Length);
            }
        }

        public virtual void DecodeObject(BinaryReader reader, RayCityObjectBuffer? buffer) 
        {
            
        }
        
        public virtual void EncodeObject(BinaryWriter writer, RayCityObjectBuffer? buffer)
        {
            
        }
    }
}