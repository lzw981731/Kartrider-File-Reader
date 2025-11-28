using KartLibrary.IO;

namespace KartCity.Common.IO.SmartStream;

public static class SmartStreamExtension
{
    public static byte[] ReadSmartStreamToBytes(this BinaryReader reader, int length)
    {
        byte[] inputData = reader.ReadBytes(length);
        return SmartStreamUtility.DecodeSmartStreamData(inputData);
    }

    public static SmartInStream ReadSmartStream(this BinaryReader reader, int length)
    {
        byte[] inputData = reader.ReadBytes(length);
        return new SmartInStream(inputData);
    }

    public static void WriteAsSmartStreamData(
        this BinaryWriter writer, byte[] data, SmartStreamMode smartStreamMode, bool writeEncodedLength, uint key = 0
    )
    {
        byte[] outputData = SmartStreamUtility.EncodeSmartStreamData(data, smartStreamMode, key: key);
        if(writeEncodedLength)
            writer.Write(outputData.Length);
        writer.Write(outputData);
    }
}