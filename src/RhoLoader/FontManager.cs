using System.Drawing.Text;
using System.IO;

namespace RhoLoader;

public static class FontManager
{
    private static PrivateFontCollection  _fontCollection;
    private static Dictionary<string, FontFamily> _privateFontFamilies = [];

    public static bool Initialize()
    {
        const string fontFolder = "Font";
        _fontCollection =  new PrivateFontCollection();
        if (!Directory.Exists(fontFolder))
            return false;
        foreach (var file in Directory.GetFiles($@"{fontFolder}\", "*.?tf"))
        {
            try
            {
                _fontCollection.AddFontFile(file);
            }
            catch
            {
                
            }
        }

        foreach (var family in _fontCollection.Families)
        {
            _privateFontFamilies.Add(family.Name, family);
        }
        return true;
    }

    public static FontFamily? TryGetFontFamily(string fontFamily)
    {
        try
        {
            return new FontFamily(fontFamily, _fontCollection) ?? new FontFamily(fontFamily);
        }
        catch
        {
            try
            {
                return new FontFamily(fontFamily);
            }
            catch
            {
                return null;
            }
        }
    }
}