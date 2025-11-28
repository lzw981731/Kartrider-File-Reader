using System.Numerics;
using System.Text;

namespace eP.Extension;

public static class StringBuilderExtension
{
    public static void AppendHexView(this StringBuilder stringBuilder, byte[] rawBytes, bool showAddress = true, string prefix = "")
    {
        long lines = (rawBytes.LongLength + 0xF) >> 4;
        int addressNums = (BitOperations.Log2((ulong)rawBytes.LongLength) + 3) >> 2;
        for (long i = 0; i < lines; i++)
        {
            long address = i << 4;
            long endAddress = Math.Min(address + 16, rawBytes.Length);
            string line = prefix;
            string addressText = $"{address:x}".PadLeft(addressNums, '0') + " | ";
            
            if(showAddress)
                line += addressText;

            for (long j = address; j < endAddress; j++)
            {
                if (j != address)
                    line += " ";
                line += $"{rawBytes[j]:X2}";
            }

            stringBuilder.AppendLine(line);
        }
    }
}