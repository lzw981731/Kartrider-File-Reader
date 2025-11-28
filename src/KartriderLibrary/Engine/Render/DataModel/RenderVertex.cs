using System.Numerics;
using System.Runtime.InteropServices;

namespace KartLibrary.Engine.Render.DataModel
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RenderVertex
    {
        public Vector3 Position;
        public Vector2 TextureCoord;
        public const uint SizeOfStruct = 20;
    }
}
