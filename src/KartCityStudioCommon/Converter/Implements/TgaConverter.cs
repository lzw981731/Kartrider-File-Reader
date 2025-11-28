using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Image = SixLabors.ImageSharp.Image;
using ImageFormat = Pfim.ImageFormat;

namespace KartCityStudio.Common.Converter.Implements;

public class TgaConverter: IFileConverter
{
    public string SourceExtension => ".tga";
    public string DestinationExtension => ".tga.png";
    public byte[] ConvertFile(byte[] orgFile)
    {
        using var stream = new MemoryStream(orgFile);
        using IImage image = Pfimage.FromStream(stream);
        using Image newImg = image.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Rgba32>(image.Data, image.Width, image.Height),
            ImageFormat.Rgba16 => Image.LoadPixelData<Bgra5551>(image.Data, image.Width, image.Height),
            ImageFormat.Rgb24 => Image.LoadPixelData<Rgb24>(image.Data, image.Width, image.Height),
            _ => throw new Exception("")
        };
        using var tmpStream = new MemoryStream();
        newImg.SaveAsPng(tmpStream);
        
        return tmpStream.ToArray();
    }
}