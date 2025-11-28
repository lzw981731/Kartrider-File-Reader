using System.IO;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using eP.Text;
using eP.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using RhoLoader.Controls.PictureViewer;
using RhoLoader.Controls.TextBoxPlus;
using Color = System.Drawing.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace RhoLoader.Dialog;

public partial class TestWindow : Form
{
    private ElementHost _elementHost;
    private PictureViewerWpf _pictureViewerWpfA;

    private Color _tagStartColor = Color.FromArgb(0x4A, 0x70, 0xA9);
    private Color _tagNameColor = Color.FromArgb(0x56, 0x83, 0xB1);
    private Color _attrAssignColor = Color.FromArgb(0x8F, 0xAB, 0xD4);
    private Color _attrNameColor = Color.FromArgb(0xBF, 0x1A, 0x1A);
    private Color _attrValueColor = Color.FromArgb(0x5C, 0x3E, 0x94);
    
    public TestWindow()
    {
        InitializeComponent();

        _pictureViewerWpfA = new PictureViewerWpf();
        
        _elementHost = new ElementHost();
        _elementHost.Child = _pictureViewerWpfA;
        _elementHost.Dock = DockStyle.Fill;
        _pictureViewerWpf.Controls.Add(_elementHost);

        // _textEditor = new TextEditor();
        // _textEditor.IsReadOnly = true;
        // _textEditor.FontFamily = new FontFamily("Consolas");
        // _textEditor.FontSize = 10;
        //
        // _elementHost = new ElementHost();
        // _elementHost.Child = _textEditor;
        // _elementHost.Dock = DockStyle.Fill;
        //
        // _textEditPanel.Controls.Add(_elementHost);


    }

    private void ActionLoad(object sender, EventArgs e)
    {
        using var autioStream = new FileStream("", FileMode.Open);
        using var stream = new FileStream("", FileMode.Open);
        
        byte[] data = new byte[autioStream.Length];
        autioStream.Read(data);
        
        // _musicPlayer.LoadMusic(data);

        // Bitmap bmp = new Bitmap("");
        // _pictureViewer.Image = bmp.Clone() as Bitmap;
        // bmp.Dispose();
        
        var imageStream = new FileStream("", FileMode.Open);

        BitmapImage bitmapSource = new BitmapImage();
        
        bitmapSource.BeginInit();
        bitmapSource.StreamSource = imageStream;
        bitmapSource.EndInit();

        _pictureViewerWpfA.ImageSource = bitmapSource;
        
        StreamReader reader = new StreamReader(stream);

        string xml = reader.ReadToEnd();

        // _textEditor.Text = xml;

        _textBoxPlus.Text = xml;
        Task.Run(() =>
        {
// _textBoxPlus.Text = "\tThis is hello world test!\r\n\tThis is hello world test!\r\n";
        List<TextBoxPlusTextColor> colors = [];
        
        // colors.AddRange([.. Enumerable.Range(29, 25).Select(x => new TextBoxPlusTextColor
        // {
        //     From = x,
        //     To = x + 1,
        //     Color = Color.FromArgb(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255))
        // })]);
        
        // colors.Add(new TextBoxPlusTextColor
        // {
        //     From = 21,
        //     To = 26,
        //     Color = Color.Blue
        // });
        
        
        XmlStyleParser styleParser = new XmlStyleParser(new XmlScanner(new TextLineReader(xml)));
        var stylizedElements = styleParser.Parse();
        
        foreach (var stylizedElement in stylizedElements)
        {
            switch (stylizedElement)
            {
                case XmlStylizedDeclarationTag declarationTag:
                    MarkString    (colors, declarationTag.TagBeginPosition, _tagStartColor);
                    MarkString    (colors, declarationTag.TagNamePosition, _tagNameColor);
                    MarkAttributes(colors, declarationTag.Attributes);
                    MarkString    (colors, declarationTag.TagEndPosition, _tagStartColor);
                    break;
                case XmlStylizedStartTag startTag:
                    MarkString    (colors, startTag.TagBeginPosition, _tagStartColor);
                    MarkString    (colors, startTag.TagNamePosition, _tagNameColor);
                    MarkAttributes(colors, startTag.Attributes);
                    MarkString    (colors, startTag.TagEndPosition, _tagStartColor);
                    break;
                case XmlStylizedEndTag endTag:
                    MarkString(colors, endTag.TagBeginPosition, _tagStartColor);
                    MarkString(colors, endTag.TagNamePosition, _tagNameColor);
                    MarkString(colors, endTag.TagEndPosition, _tagStartColor);
                    break;
                case XmlStylizedCommentTag commentTag:
                    MarkString(colors, commentTag.CommentPosition, Color.DarkGreen);
                    break;
            }
        }
        
        _textBoxPlus.InitForeColor(colors, true);
        });
        
    }
    
    private void MarkAttributes(List<TextBoxPlusTextColor> output, IEnumerable<(Range key, Range? assign, Range? value)> attributes)
    {
        foreach (var attr in attributes)
        {
            MarkString(output, attr.key, _attrNameColor);
            MarkString(output, attr.assign, _attrAssignColor);
            MarkString(output, attr.value, _attrValueColor);
        }
    }
	    
    private void MarkString(List<TextBoxPlusTextColor> output, Range? range, Color color)
    {
        if (range is null)
            return;

        output.Add(new TextBoxPlusTextColor()
        {
            From = range.Value.Start.Value,
            To = range.Value.End.Value,
            Color = color
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        _elementHost.Dispose();
    }
}