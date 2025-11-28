using System.Numerics;
using System.Runtime.InteropServices;

namespace KartLibrary.Engine.Render.DataModel;

[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct RenderMesh
{
    public Vector3 Position;
    public Vector3 NormalVec;
    public Vector3 TexCoord;
    public const uint SizeOfStruct = 36;
}