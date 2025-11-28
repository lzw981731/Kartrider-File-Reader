using System.Numerics;

namespace eP.Extension;

public static class BinaryWriterExtension
{
    public static void Write(this BinaryWriter writer, Vector2 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
    }
    
    public static void Write(this BinaryWriter writer, Vector3 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
        writer.Write(vec.Z);
    }
}