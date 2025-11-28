using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace KartLibrary.Engine.Render.DataModel
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct AlphaPropertyInfo
    {
        public bool AlphaTestEnabled;
        public ComparisonFunction AlphaTestFunction;
        public int AlphaTestRef;

        public const uint SizeOfStruct = 16;
    }
}
