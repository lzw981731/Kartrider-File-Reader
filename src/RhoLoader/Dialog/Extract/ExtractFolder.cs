using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Pfim;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;
using KartCity.Common.Xml;
using KartCityStudio.Common.Converter.Implements;
using KartCityStudio.Common.Utility;
using KartCityStudio.Game.Model;
using RhoLoader.Setting;

using KartLibrary.File;

namespace RhoLoader
{
    public partial class ExtractFolder : Form
    {
        private static class ExtractConverter
        {
            public static byte[] DDSConverter(byte[] inputData)
            {
                var stream = new MemoryStream(inputData);
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
                stream.Dispose();
                stream = new MemoryStream();
                Bitmap bmp = new Bitmap(image.Width, image.Height, image.Stride, pf, d);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] output = ms.ToArray();

                handle.Free();
                image.Dispose();
                ms.Dispose();
                bmp.Dispose();

                return output;
            }

            public static byte[] BMLConverter(byte[] inputData)
            {
                BinaryXmlDocument bxd = new BinaryXmlDocument();
                bxd.Read(Encoding.GetEncoding("UTF-16"), inputData);
                BinaryXmlTag bxt = bxd.RootTag;
                string xmlData = bxt.ToString();
                byte[] output = Encoding.UTF8.GetBytes(xmlData);
                return output;
            }

            public static byte[] KSVConverter(byte[] inputData)
            {
                return inputData;
            }

            public static byte[] NoneConvert(byte[] inputData)
            {
                return inputData;
            }
        }

        private ExtractOptionToken _extractOption;
        private ArchiveExtractor _extractor;

        private CancellationTokenSource _tokenSource = new();
        

        public ExtractFolder(string extractPath, ExtractOptionToken extractOption, params IArchiveElement[] extractElements)
        {
            InitializeComponent();
            _extractOption = extractOption;
            _extractor = new ArchiveExtractor(extractPath, extractElements);
            _extractor.ProgressReport += ExtractorOnProgressReport;
            _extractor.ExtractCompleted += ExtractorOnExtractCompleted;
            _extractor.ExtractFailured += ExtractorOnExtractFailured;
            if(extractOption.HasFlag(ExtractOptionToken.ConvertBml))
                _extractor.AddConverter(new BmlConverter());
            if(extractOption.HasFlag(ExtractOptionToken.ConvertDds))
                _extractor.AddConverter(new DdsConverter());
            if(extractOption.HasFlag(ExtractOptionToken.ConvertTga))
                _extractor.AddConverter(new TgaConverter());
        }

        private void BeginExtract()
        {
            _extractor.BeginExtract(_tokenSource.Token);
        }

        private void FinishExtract()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(FinishExtract);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void TerminateExtract()
        {
            _tokenSource.Cancel();
            if (this.InvokeRequired)
            {
                this.Invoke(TerminateExtract);
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void ReportProgress(ExtractState state, string fileName, int extractedCount, int totalCount, double progress)
        {
            if (this.InvokeRequired)
            {
                Action<ExtractState, string, int, int, double> action = ReportProgress;
                this.Invoke(action, [state, fileName, extractedCount, totalCount, progress]);
            }
            else
            {
                textExtractFile.Text = fileName;
                textProgress.Text = $"{extractedCount}/{totalCount}";
                progressMain.Value = progress;
            }
        }

        private void ExtractorOnProgressReport(object sender, ExtractProgressEventArgs e)
        {
            ReportProgress(e.State, e.CurrentDumpFileName, e.ExtractedFilesCount, e.TotalFilesCount, e.Progress);
        }
        
        private void ExtractorOnExtractCompleted()
        {
            FinishExtract();
        }

        private void ExtractorOnExtractFailured(Exception obj)
        {
            MessageBox.Show($"Extract exception: {obj.Message}\r\nStack trace: \r\n{obj.StackTrace}");
        }

        // actions
        private void actionShow(object sender, EventArgs e)
        {
            BeginExtract();
        }

        private void actionCancel(object sender, EventArgs e)
        {
            if(MessageBox.Show("msg_cancelExtract".GetStringBag(), "msg_level_question".GetStringBag(), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TerminateExtract();
            }
        }
    }
}
