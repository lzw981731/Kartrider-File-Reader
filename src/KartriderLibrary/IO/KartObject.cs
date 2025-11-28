using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.IO;

namespace KartLibrary.IO
{
    /// <summary>
    /// Represent the::Object in KartRider.
    /// It is a serializable object. 
    /// </summary>
    public abstract class KartObject
    {
        protected KartObject() 
        {
            //if (!KartObjectManager.ContainsClass(ClassStamp))
            //    KartObjectManager.RegisterClass(this.GetType());
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

        public virtual void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer) 
        {
            
        }
        
        public virtual void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            
        }
    }
}
