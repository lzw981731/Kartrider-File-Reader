using System.Runtime.InteropServices;

namespace KartLibrary.Engine.Render.DataModel;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct ToonPropertyInfo
{
    public float TexAlpha;
    public float TexOffsetX;
    public float TexOffsetY;
    private float _padding1;

    public static uint SizeOfStruct = 16u;
}