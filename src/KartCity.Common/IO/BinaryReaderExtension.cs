using System.Net;
using System.Numerics;
using System.Text;
using KartCity.Common.Engine;

namespace KartCity.Common.IO;

public static class BinaryReaderExtension
{
    public static string ReadNullTerminatedText(this BinaryReader br, bool wideString)
    {
        StringBuilder stringBuilder = new StringBuilder(16);
        if (wideString)
        {
            char ch;
            while((ch = (char)br.ReadInt16()) != '\0')
                stringBuilder.Append(ch);
        }
        else
        {
            char ch;
            while ((ch = (char)br.ReadByte()) != '\0')
                stringBuilder.Append(ch);
        }
        return stringBuilder.ToString();
    }

    public static Vector2 ReadVector2(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        return new Vector2(x, y);
    }

    public static Vector3 ReadVector3(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        return new Vector3(x, y, z);
    }

    public static Vector4 ReadVector4(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        float w = br.ReadSingle();
        return new Vector4(x, y, z, w);
    }

    public static Vector2[] ReadVector2Array(this BinaryReader br)
    {
        Vector2[] output = new Vector2[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadVector2();
        }

        return output;
    }
    
    public static Vector3[] ReadVector3Array(this BinaryReader br)
    {
        Vector3[] output = new Vector3[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadVector3();
        }

        return output;
    }
    
    public static Vector4[] ReadVector4Array(this BinaryReader br)
    {
        Vector4[] output = new Vector4[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadVector4();
        }

        return output;
    }
    
    public static short[] ReadInt16Array(this BinaryReader br)
    {
        short[] output = new short[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadInt16();
        }

        return output;
    }
    
    public static int[] ReadInt32Array(this BinaryReader br)
    {
        int[] output = new int[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadInt32();
        }

        return output;
    }
    
    public static long[] ReadInt64Array(this BinaryReader br)
    {
        long[] output = new long[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadInt64();
        }

        return output;
    }
    
    public static float[] ReadSingleArray(this BinaryReader br)
    {
        float[] output = new float[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadSingle();
        }

        return output;
    }
    
    public static double[] ReadDoubleArray(this BinaryReader br)
    {
        double[] output = new double[br.ReadInt32()];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = br.ReadDouble();
        }

        return output;
    }
    
    public static BoundingBox ReadBoundBox(this BinaryReader br)
    {
        Vector3 minPos = br.ReadVector3();
        Vector3 maxPos = br.ReadVector3();
        return new BoundingBox(minPos, maxPos);
    }

    public static IPEndPoint ReadIPv4EndPoint(this BinaryReader reader)
    {
        byte[] ipv4Addr = reader.ReadBytes(4);
        ushort port = reader.ReadUInt16();
        return new IPEndPoint(new IPAddress(ipv4Addr), port);
    }

    public static DateTime ReadKRDateTime(this BinaryReader br)
    {
        DateTime dt = new DateTime(1900, 1, 1);
        uint date = (uint)br.ReadUInt16();
        uint time = (uint)br.ReadUInt16() * 4;
        dt = dt.AddDays(date);
        dt = dt.AddSeconds(time);
        return dt;
    }

    public static string ReadKRString(this BinaryReader br)
    {
        return br.ReadKRString(Encoding.Unicode);
    }

    public static KeyValuePair<string, string> ReadKRStringPair(this BinaryReader br)
    {
        return new KeyValuePair<string, string>(
            br.ReadKRString(),
            br.ReadKRString()
        );
    }

    public static string ReadKRString(this BinaryReader br, Encoding encoding)
    {
        int len = br.ReadInt32();
        byte[] strData = br.ReadBytes(len << 1);
        return encoding.GetString(strData);
    }

    public static string ReadKrAsciiString(this BinaryReader br)
    {
        int len = br.ReadInt32();
        byte[] strData = br.ReadBytes(len);
        return Encoding.ASCII.GetString(strData);
    }
}