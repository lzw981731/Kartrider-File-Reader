using System.IO;
using System.Text;
using KartCity.Common.Xml;
using RhoLoader.XML;

namespace RhoLoader.Controls.PreviewPanel;

public partial class XmlPreview : UserControl
{
    public XmlPreview()
    {
        InitializeComponent();
    }

    public void InitControl(Stream fileStream, bool isBml,  CancellationToken token)
    {
        string xml = "";

        if (isBml)
        {
            BinaryReader reader = new BinaryReader(fileStream);
            xml = reader.ReadBinaryXmlTag(Encoding.Unicode).ToString();
        }
        else
        {
            StreamReader reader = new StreamReader(fileStream);
            xml = reader.ReadToEnd();
        }
        
        _textBoxPlus.Text = xml;
        
        fileStream.Dispose();
        
        Task.Run(() => _textBoxPlus.ApplyXmlStyle(xml, token), token);
    }
}