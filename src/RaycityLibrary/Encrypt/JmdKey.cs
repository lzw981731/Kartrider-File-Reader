using System.Text;
using KartCity.Common.IO;

namespace RayCityLibrary.Encrypt
{
    public static class JmdKey
    {
        public static uint GetJmdKey(string fileName)
        {
            byte[] stringData = Encoding.GetEncoding("UTF-16").GetBytes(fileName);
            return Adler.Adler32(0, stringData, 0, stringData.Length) + 0x3de90dc3;
        }

        public static uint GetBlockFirstKey(uint rhoKey)
        {
            return rhoKey ^ 0x3A9213AC;
        }

        public static uint GetDirectoryDataKey(uint rhoKey)
        {
            return rhoKey - 0x41014EBF;
        }

        public static uint GetFileKey(uint jmdKey, string fileName, uint extNum)
        {
            byte[] strData = Encoding.GetEncoding("UTF-16").GetBytes(fileName);
            uint key = Adler.Adler32(0, strData, 0, strData.Length);
            key += extNum;
            key += (jmdKey - 0x7E2AF33D);
            return key;
        }

        public static unsafe byte[] ExtendKey(uint originalKey)
        {
            byte[] outArray = new byte[64];
            fixed(byte* wPtr = outArray)
            {
                uint *writePtr = (uint*)wPtr;
                uint curData = originalKey ^ 0x8473fbc1;
                for (int i = 0; i < 16; i++)
                {
                    writePtr[i] = curData;
                    curData -= 0x7b8c043f;
                }
            }
            return outArray;
        }
    }
}