using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Tontrollers
{
    public interface IKeyData<TValue>
    {
        TValue GetValue(float time);

        void DecodeObject(BinaryReader reader, int count);
    }
}
