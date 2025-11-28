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
using KartCity.Common.Xml;
using RhoLoader.XML;

namespace RhoLoader.PreviewWindow
{
    public partial class BmlViewer : Form
    {
        private string _rawXml = "";
        private string _saveFileName = "";
        
        public BmlViewer()
        {
            InitializeComponent();
        }

        public void LoadFromBml(string saveFileName, BinaryXmlTag bmlTag)
        {
            _saveFileName = saveFileName;
            _rawXml = bmlTag.ToString();
            
            bmlTag.ApplyToRichTextBox(_richTextBox);
        }
        
        public void LoadFromXml(string saveFileName, string xml)
        {
            _saveFileName = saveFileName;
            _rawXml = xml;
            
            xml.StylizeXmlToRichText(_richTextBox);
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
