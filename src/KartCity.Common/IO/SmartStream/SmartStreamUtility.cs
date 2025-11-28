using ClassLibrary1.Encrypt;
using Ionic.Zlib;
using KartLibrary.IO;
using Adler = KartCity.Common.IO.Adler;

namespace KartCity.Common.IO.SmartStream;
public static class SmartStreamUtility
{
    public static byte[] DecodeSmartStreamData(byte[] smartStreamData)
    {
        if (smartStreamData.Length < 6 || smartStreamData[0] != 0x53)
            throw new Exception("Given data isn't smart stream data.");
        using (MemoryStream ms = new MemoryStream(smartStreamData))
        {
            BinaryReader reader = new BinaryReader(ms);
            reader.ReadByte();
            SmartStreamMode smartStreamMode = (SmartStreamMode)reader.ReadByte();
            bool isEncrypted = smartStreamMode.HasFlag(SmartStreamMode.Encrypted);
            bool isCompressed = smartStreamMode.HasFlag(SmartStreamMode.Compressed);
            uint dataChecksum = reader.ReadUInt32();
            uint encryptKey = isEncrypted
                ? reader.ReadUInt32()
                : 0;
            int decompressSize = isCompressed
                ? reader.ReadInt32()
                : 0;
            byte[] rawData = reader.ReadBytes((int)(smartStreamData.Length - ms.Position));
            if (isEncrypted)
                rawData = SmartStreamEncrypt.DecryptData(encryptKey, rawData);
            if (isCompressed)
                using (MemoryStream tmpMs = new MemoryStream(rawData))
                {
                    rawData = new byte[decompressSize];
                    ZlibStream zlibStream = new ZlibStream(tmpMs, CompressionMode.Decompress);
                    zlibStream.Read(rawData, 0, rawData.Length);
                }

            uint decDataChecksum = Adler.Adler32(0, rawData, 0, rawData.Length);
            if (decDataChecksum != dataChecksum)
                throw new Exception("SmartStream data is corrupted.");
            return rawData;
        }
    }

    public static byte[] EncodeSmartStreamData(byte[] inData, SmartStreamMode mode, uint key = 0)
    {
        uint dataChecksum = Adler.Adler32(0, inData, 0, inData.Length);
        int decompressSize = inData.Length;
        bool isEncrypted = mode.HasFlag(SmartStreamMode.Encrypted);
        bool isCompressed = mode.HasFlag(SmartStreamMode.Compressed);
        byte[] rawData = inData;
        if(isCompressed)
            using (MemoryStream tmpMs = new MemoryStream())
            {
                ZlibStream zlibStream = new ZlibStream(tmpMs, CompressionMode.Compress);
                zlibStream.Write(rawData, 0, rawData.Length);
                // zlibStream.Flush();
                zlibStream.Close();
                rawData = tmpMs.ToArray();
            }

        if (isEncrypted)
            rawData = SmartStreamEncrypt.EncryptData(key, rawData);
        using (MemoryStream tmpMs = new MemoryStream(rawData.Length + 14))
        {
            BinaryWriter writer = new BinaryWriter(tmpMs);
            writer.Write((byte) 0x53);
            writer.Write((byte) mode);
            writer.Write(dataChecksum);
            if(isEncrypted)
                writer.Write(key);
            if(isCompressed)
                writer.Write(decompressSize);
            writer.Write(rawData);
            return tmpMs.ToArray();
        }
    }
}