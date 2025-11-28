using eP.Text;
using eP.Xml;
using RhoLoader.Controls.TextBoxPlus;

namespace RhoLoader.XML;

public static class TextBoxPlusExtension
{
    private static Color TagStartColor = Color.FromArgb(0x4A, 0x70, 0xA9);
    private static Color TagNameColor = Color.FromArgb(0x56, 0x83, 0xB1);
    private static Color AttrAssignColor = Color.FromArgb(0x8F, 0xAB, 0xD4);
    private static Color AttrNameColor = Color.FromArgb(0xBF, 0x1A, 0x1A);
    private static Color AttrValueColor = Color.FromArgb(0x5C, 0x3E, 0x94);

    public static void ApplyXmlStyle(this TextBoxPlus textBoxPlus, string xml, CancellationToken token = default)
    {
        ApplyXmlStyleInternal(textBoxPlus, xml, token);
    }
    
    public static async Task ApplyXmlStyleAsync(this TextBoxPlus textBoxPlus, string xml, CancellationToken token = default)
    {
        await Task.Run(() => ApplyXmlStyleInternal(textBoxPlus, xml, token), token);
    }
    
    private static void ApplyXmlStyleInternal(this TextBoxPlus textBoxPlus, string xml, CancellationToken token)
    {
        List<TextBoxPlusTextColor> colors = [];
        XmlStyleParser styleParser = new XmlStyleParser(new XmlScanner(new TextLineReader(xml)));
        var stylizedElements = styleParser.Parse();
        
        foreach (var stylizedElement in stylizedElements)
        {
            token.ThrowIfCancellationRequested();
            
            switch (stylizedElement)
            {
                case XmlStylizedDeclarationTag declarationTag:
                    MarkString    (colors, declarationTag.TagBeginPosition, TagStartColor);
                    MarkString    (colors, declarationTag.TagNamePosition, TagNameColor);
                    MarkAttributes(colors, declarationTag.Attributes, token);
                    MarkString    (colors, declarationTag.TagEndPosition, TagStartColor);
                    break;
                case XmlStylizedStartTag startTag:
                    MarkString    (colors, startTag.TagBeginPosition, TagStartColor);
                    MarkString    (colors, startTag.TagNamePosition, TagNameColor);
                    MarkAttributes(colors, startTag.Attributes, token);
                    MarkString    (colors, startTag.TagEndPosition, TagStartColor);
                    break;
                case XmlStylizedEndTag endTag:
                    MarkString(colors, endTag.TagBeginPosition, TagStartColor);
                    MarkString(colors, endTag.TagNamePosition, TagNameColor);
                    MarkString(colors, endTag.TagEndPosition, TagStartColor);
                    break;
                case XmlStylizedCommentTag commentTag:
                    MarkString(colors, commentTag.CommentPosition, Color.DarkGreen);
                    break;
            }
        }
        
        token.ThrowIfCancellationRequested();
        
        textBoxPlus.InitForeColor(colors, true);
    }
    
    private static void MarkAttributes(List<TextBoxPlusTextColor> output, IEnumerable<(Range key, Range? assign, Range? value)> attributes, CancellationToken token)
    {
        foreach (var attr in attributes)
        {
            token.ThrowIfCancellationRequested();
            
            MarkString(output, attr.key, AttrNameColor);
            MarkString(output, attr.assign, AttrAssignColor);
            MarkString(output, attr.value, AttrValueColor);
        }
    }
	
    private static void MarkString(List<TextBoxPlusTextColor> output, Range? range, Color color)
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
}