using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pfim;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using RhoLoader.Controls.PictureViewer;
using ImageFormat = Pfim.ImageFormat;

namespace RhoLoader.PreviewWindow
{
    public partial class ImageViewer : Form
    {
        private ElementHost _elementHost;
        private PictureViewerWpf _pictureViewer;
        private Bitmap? _baseBitmap;
        
        public ImageViewer()
        {
            InitializeComponent();

            _pictureViewer = new PictureViewerWpf();

            _elementHost = new ElementHost();
            _elementHost.Child = _pictureViewer;
            _elementHost.Dock = DockStyle.Fill;
            _pictureViewerPanel.Controls.Add(_elementHost);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] Data { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileType Type { get; set; }

        private double _scaleN = 1.0d;

        public enum FileType
        {
            dds,tga
        }
        
        GCHandle handle;
        public void ShowBox()
        {
            this.Show();
            using(MemoryStream ms = new MemoryStream(Data))
            {
                IImage image = Pfim.Pfimage.FromStream(ms);
                handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
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
                _baseBitmap = new Bitmap(image.Width,image.Height,image.Stride,pf,d);
                MemoryStream memoryStream = new MemoryStream();
                _baseBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                _pictureViewer.ImageSource = bitmapImage;
                
                _scaleN = 1;
                scale.Text = $"{_scaleN:00.00x}";
            }
        }

        ~ImageViewer()
        {
            handle.Free();

        }
        bool Dark = false;
        private void ActionTurnToDark(object sender, EventArgs e)
        {
            if (Dark)
            {
                _pictureViewerPanel.BackColor = Color.FromArgb(240, 240, 240);
                Dark = false;
                turnToDarkBackgroundToolStripMenuItem.Text = "Turn to Dark Background";
                return;
            }
            _pictureViewerPanel.BackColor = Color.FromArgb(31, 31, 31);
            Dark = true;
            turnToDarkBackgroundToolStripMenuItem.Text = "Turn to Light Background";
        }

        private void saveToPngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog savePng = new SaveFileDialog
            {
                Filter = "PNGFile|*.png",
                Title = "Select the location you want to save."
            };
            if(savePng.ShowDialog() == DialogResult.OK)
            {
                _baseBitmap?.Save(savePng.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void ConvertTGADDSToPng()
        {
            using (MemoryStream ms = new MemoryStream(Data))
            {
                IImage image = Pfim.Pfimage.FromStream(ms);
                handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
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
                Bitmap bmp = new Bitmap(image.Width, image.Height, image.Stride, pf, d);
                SaveFileDialog savePng = new SaveFileDialog
                {
                    Filter = "PNGFile|*.png",
                    Title = "Select the location you want to save."
                };
                if (savePng.ShowDialog() == DialogResult.OK)
                {
                    bmp.Save(savePng.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            _elementHost.Dispose();
        }
    }
}
