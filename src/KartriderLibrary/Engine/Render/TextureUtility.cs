using System.Numerics;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Veldrid;

namespace KartLibrary.Engine.Render
{
    public static class TextureUtility
    {
        public unsafe static Texture CreateTexture(GraphicsDevice graphicsDevice, string imagePath)
        {
            ResourceFactory factory = graphicsDevice.ResourceFactory;
            Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
            Image<Rgba32> convImg = image.CloneAs<Rgba32>();
            TextureDescription texDesc = 
                new TextureDescription(
                    (uint)image.Width, (uint)image.Height,
                    1, 1, 1, 
                    PixelFormat.R8_UNorm, TextureUsage.Sampled, TextureType.Texture2D, TextureSampleCount.Count32);
            TextureDescription texCpyDesc =
                 new TextureDescription(
                     (uint)image.Width, (uint)image.Height, 1, 1, 1, PixelFormat.R8_UNorm, TextureUsage.Staging, TextureType.Texture2D, TextureSampleCount.Count32);

            Texture texture = factory.CreateTexture(texDesc);
            Texture staging = factory.CreateTexture(texCpyDesc);
            byte[] pixels = new byte[image.Width * image.Height * 4];
            convImg.CopyPixelDataTo(pixels);
            fixed(byte* src = pixels)
            {
                graphicsDevice.UpdateTexture(staging, (nint)src, (uint)pixels.Length, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
            }
            CommandList cl = factory.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, texture);
            cl.End();
            graphicsDevice.SubmitCommands(cl);
            cl.Dispose();
            return texture;
        }
        
        public unsafe static Texture CreateTexture(GraphicsDevice graphicsDevice, CommandList cl, Stream stream)
        {
            ResourceFactory factory = graphicsDevice.ResourceFactory;
            Image<Rgba32> image = Image.Load<Rgba32>(stream);
            return CreateTexture(graphicsDevice, cl, image);
        }
        
        public unsafe static Texture CreateTexture(GraphicsDevice graphicsDevice, CommandList cl, Image image)
        {
            ResourceFactory factory = graphicsDevice.ResourceFactory;
            image = image.CloneAs<Rgba32>();
            int mipLevel = BitOperations.Log2((uint)Math.Min(image.Width, image.Height)); 
            TextureDescription texDesc = 
                new TextureDescription((uint)image.Width, (uint)image.Height, 1, (uint)mipLevel, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D);
            TextureDescription texCpyDesc =
                new TextureDescription((uint)image.Width, (uint)image.Height, 1, (uint)mipLevel, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D);
            
            Texture texture = factory.CreateTexture(texDesc);
            Texture staging = factory.CreateTexture(texCpyDesc);
            for (int i = 0; i < mipLevel; i++)
            {
                if (i > 0)
                {
                    image.Mutate(x =>
                    {
                        x.Resize(new Size(image.Width >> 1, image.Height >> 1), new BicubicResampler(), true);
                    });
                }
                byte[] pixels = new byte[image.Width * image.Height * 4];
                ((Image<Rgba32>) image).CopyPixelDataTo(pixels);
                fixed(byte* src = pixels)
                {
                    graphicsDevice.UpdateTexture(staging, (nint)src, (uint)pixels.Length, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, (uint)i, 0);
                }
            }
            cl.CopyTexture(staging, texture);
            return texture;
        }
        
        public unsafe static Texture CreateTexture(GraphicsDevice graphicsDevice, CommandList cl, Stream stream, bool isDDs)
        {
            if (!isDDs)
                return CreateTexture(graphicsDevice, cl, stream);
            IImage loadTexture = Pfimage.FromStream(stream);
            Image image = loadTexture.Format switch
            {
                ImageFormat.Rgb8 => Image.LoadPixelData<L8>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.R5g5b5 => Image.LoadPixelData<Bgra5551>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.R5g5b5a1 => Image.LoadPixelData<Bgra5551>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.R5g6b5 => Image.LoadPixelData<Bgr565>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.Rgba16 => Image.LoadPixelData<Bgra4444>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.Rgb24 => Image.LoadPixelData<Bgr24>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(loadTexture.Data, loadTexture.Width, loadTexture.Height),
                _ => throw new Exception()
            };
            return CreateTexture(graphicsDevice, cl, image);
        }
    }
}
