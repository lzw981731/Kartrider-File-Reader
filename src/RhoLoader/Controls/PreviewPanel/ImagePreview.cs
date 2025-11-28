using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using Pfim;
using RhoLoader.Controls.PictureViewer;

namespace RhoLoader.Controls.PreviewPanel;

public partial class ImagePreview : UserControl
{
    private ElementHost _elementHost;
    private PictureViewerWpf _pictureViewer;
    private Stream? _baseStream;
    
    public ImagePreview()
    {
        InitializeComponent();

        _pictureViewer = new PictureViewerWpf();

        _elementHost = new ElementHost();
        _elementHost.Dock = DockStyle.Fill;
        _elementHost.Child = _pictureViewer;

        this.Controls.Add(_elementHost);
        
        Disposed += OnDisposed;
    }

    public void InitControl(Stream stream, bool reqConvert, CancellationToken token)
    {
        Stream targetStream;
        if (reqConvert)
        {
            IImage image = Pfim.Pfimage.FromStream(stream);
            var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            var d = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
            PixelFormat pf;
            switch (image.Format)
            {
                case Pfim.ImageFormat.Rgba32:
                    pf = PixelFormat.Format32bppArgb;
                    break;
                case Pfim.ImageFormat.Rgba16:
                    pf = PixelFormat.Format16bppArgb1555;
                    break;
                case Pfim.ImageFormat.Rgb8:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case Pfim.ImageFormat.Rgb24:
                    pf = PixelFormat.Format24bppRgb;
                    break;
                default:
                    throw new Exception("");
            }
            Bitmap bmp = new Bitmap(image.Width,image.Height,image.Stride,pf,d);
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            targetStream = memoryStream;
            
            handle.Free();
            bmp.Dispose();
        }
        else
        {
            targetStream = stream;
        }
        

        _pictureViewer.Dispatcher.Invoke(() =>
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = targetStream;
            bitmapImage.EndInit();
        
            _pictureViewer.ImageSource = bitmapImage;
        });

        _baseStream = targetStream;
    }

    private void OnDisposed(object? sender, EventArgs e)
    {
        // _baseStream?.Dispose();
    }
}