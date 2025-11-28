using System.Text;

namespace KartCity.Common.IO
{
    public static class Adler
    {
        public const uint AdlerModulo = 65521;
        public static uint Adler32(uint adler, ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < (buffer.Length))
                throw new Exception("buffer is small.");
            uint a = adler & 0xFFFFu;
            uint b = (adler >> 16) & 0xFFFFu;
            for(int i = 0; i < buffer.Length; i++)
            {
                a = (a + buffer[i]) % AdlerModulo;
                b = (b + a) % AdlerModulo;
            }
            return (b << 16) | a;
        }

        public static uint Adler32(uint adler, string str, Encoding encoding)
        {
            return Adler32(adler, encoding.GetBytes(str));
        }
        
        public static uint Adler32(uint adler, byte[] buffer, int offset, int count)
        {
            if(buffer.Length < (offset + count))
                throw new Exception("buffer is small.");
            return Adler32(adler, buffer[offset..(offset+count)]);
        }
        
        public static uint Adler32Combine(uint prevChksum, byte[] buffer, int offset, int count)
        {
            uint a = prevChksum & 0xFFFFu;
            uint b = (prevChksum >> 16) & 0xFFFFu;
            for (int i = 0; i < count; i++)
            {
                a = (a + buffer[offset + i]) % AdlerModulo;
                b = (b + a) % AdlerModulo;
            }
            return (b << 16) | a;
        }
    }
}
