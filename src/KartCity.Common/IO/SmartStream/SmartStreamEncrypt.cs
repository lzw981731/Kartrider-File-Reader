using System.Runtime.CompilerServices;

namespace ClassLibrary1.Encrypt;

public static class SmartStreamEncrypt
{
    /// <summary>
    /// Used to decrypt SmartStream data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] DecryptData(uint key,byte[] data)
    {
        byte[] extendedKey = extendKey(key);
        byte[] output = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            output[i] = (byte)(data[i] ^ extendedKey[i & 63]);
        }
        return output;
    }

    /// <summary>
    /// Used to decrypt rho file data, or DataProcessed data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] EncryptData(uint key, byte[] data)
    {
        return DecryptData(key, data);
    }
    
    private static unsafe byte[] extendKey(uint originalKey)
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