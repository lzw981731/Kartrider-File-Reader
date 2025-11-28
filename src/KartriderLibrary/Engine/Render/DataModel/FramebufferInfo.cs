using System.Runtime.InteropServices;

namespace KartLibrary.Engine.Render.DataModel;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct FramebufferInfo
{
    public int SampleCount;
    
    public const uint SizeOfStruct = 16;
}