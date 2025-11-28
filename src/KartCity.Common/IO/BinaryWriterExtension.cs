using System.Net;
using System.Numerics;
using System.Text;

namespace KartCity.Common.IO;

public static class BinaryWriterExtension
{
    public static void WriteKRDateTime(this BinaryWriter bw, DateTime dateTime)
    {
        DateTime dt = new DateTime(1900, 1, 1);
        TimeSpan ts = dateTime - dt;
        uint date = (uint)ts.TotalDays;
        uint time = (uint)(dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second) >> 2;
        bw.Write((ushort)date);
        bw.Write((ushort)time);
    }

    public static void WriteKRString(this BinaryWriter bw, string str)
    {
        bw.WriteKRString(Encoding.Unicode, str);
    }

    public static void WriteKRString(this BinaryWriter bw, Encoding encoding, string str)
    {
        int len = str.Length;
        byte[] strData = encoding.GetBytes(str);
        bw.Write(len);
        bw.Write(strData);
    }

    public static void WriteNullTerminatedText(this BinaryWriter bw, string text, bool wideString)
    {
        if (!wideString)
        {
            byte[] encData = Encoding.ASCII.GetBytes(text);
            bw.Write(encData);
            bw.Write((byte)0x00);
        }
        else
        {
            byte[] encData = Encoding.Unicode.GetBytes(text);
            bw.Write(encData);
            bw.Write((short)0x00);
        }
    }

    public static void WriteKRStringPair(this BinaryWriter bw, string key, string value)
    {
        bw.WriteKRString(key);
        bw.WriteKRString(value);
    }
    
    public static void WriteKRStringPair(this BinaryWriter bw,Encoding encoding, string key, string value)
    {
        bw.WriteKRString(encoding, key);
        bw.WriteKRString(encoding, value);
    }

    public static void WriteIPv4EndPoint(this BinaryWriter writer, IPEndPoint endPoint)
    {
        byte[] ipAddr = new byte[4];
        if (!endPoint.Address.TryWriteBytes(ipAddr, out _))
            throw new Exception($"can't write endpoint: {endPoint}");
        writer.Write(ipAddr);
        writer.Write((short)endPoint.Port);
    }

    public static void WriteVector3(this BinaryWriter writer, Vector3 vector3)
    {
        writer.Write(vector3.X);
        writer.Write(vector3.Y);
        writer.Write(vector3.Z);
    }
}