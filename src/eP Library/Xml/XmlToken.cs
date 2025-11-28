using System.Runtime.CompilerServices;

namespace eP.Xml;

public struct XmlToken
{
    public XmlTokenCategory Category;
    public int EndPosition;
    public int StartPosition;
    
#if SCANNER_USE_RAWSTRING
    public string RawString { get; set; } = "";
#endif

    public XmlToken()
    {
        Category = XmlTokenCategory.Invalid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invalidation()
    {
        Category = XmlTokenCategory.Invalid;
    }
}