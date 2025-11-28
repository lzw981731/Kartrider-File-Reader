using System.Runtime.InteropServices;

namespace KartLibrary.Engine.Render.DataModel
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct TexPropertyInfo
    {
        public float TexAlpha;
        public float TexOffsetX;
        public float TexOffsetY;
        public int TexU3;
        private float _padding1;

        public const uint SizeOfStruct = 32;

        public TexPropertyInfo()
        {
            TexAlpha = 1.0f;
            TexU3 = 0;
        }
    }
}
