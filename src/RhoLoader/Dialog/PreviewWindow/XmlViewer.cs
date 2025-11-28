using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KartLibrary.Xml;
using System.IO;
using System.Diagnostics;
using eP.Text;
using eP.Xml;
using KartCity.Common.Xml;
using RhoLoader.Controls.TextBoxPlus;
using RhoLoader.XML;

namespace RhoLoader.PreviewWindow
{
    public partial class XmlViewer : Form
    {
        private string _rawXml = "";
        private string _saveFileName = "";
        
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        public XmlViewer()
        {
            InitializeComponent();
        }

        public void LoadFromBml(string saveFileName, BinaryXmlTag bmlTag)
        {
            _saveFileName = saveFileName;
            _rawXml = bmlTag.ToString();

            textBoxPlus1.Text = _rawXml;

            Task.Run(() => textBoxPlus1.ApplyXmlStyle(_rawXml, _cancellationTokenSource.Token));
        }
        
        public void LoadFromXml(string saveFileName, string xml)
        {
            _saveFileName = saveFileName;
            _rawXml = xml;
            
            textBoxPlus1.Text = _rawXml;
            
            Task.Run(() => textBoxPlus1.ApplyXmlStyle(_rawXml, _cancellationTokenSource.Token));
        }
        
        private void ActionSave(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.xml|XML File";
            sfd.FileName = $"{_saveFileName}.xml";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                byte[] data = Encoding.GetEncoding("UTF-16").GetBytes(_rawXml);
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
        }

        private void ActionLoad(object sender, EventArgs e)
        {
            
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
