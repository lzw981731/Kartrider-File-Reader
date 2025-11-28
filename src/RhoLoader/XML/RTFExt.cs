using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eP.Command;
using KartLibrary.Xml;
using eP.Text;
using eP.Xml;
using KartCity.Common.Xml;
using RhoLoader.Text;

namespace RhoLoader.XML
{
    public static class RTFExt
    {
        public static void ApplyToRichTextBox(this BinaryXmlTag bxt,System.Windows.Forms.RichTextBox rtb)
        {
            RichTextFormater tf = new RichTextFormater()
            {
                LevelDelta = 4
            };
            bxt.ApplyToRichText(tf, 0);
            tf.StartFormat(rtb);
        }
        public static void ApplyToRichText(this BinaryXmlTag bxt, RichTextFormater formater, int nowLevel)
        {
            bool HaveText = bxt.Text != null && bxt.Text != "";
            bool HaveAttributes = bxt.Attributes.Count > 0;
            bool HaveSubTag = bxt.Children.Count > 0;
            string Start = "";
            string Att = "";
            string End = "";
            string addition = "";
            bool OneLine = true;
            if ((HaveText || HaveSubTag))
            {
                End = $"\\cf1 </\\cf2 {bxt.Name}\\cf1 >";
                OneLine = !HaveSubTag;
            }
            else
            {
                End = $"";
                OneLine = true;
                addition = "/";
            }
            if (HaveAttributes)
            {
                List<string> attFormat = new List<string>();
                foreach (KeyValuePair<string, string> KeyPair in bxt.Attributes)
                {
                    attFormat.Add($"\\cf4 {KeyPair.Key}\\cf3 =\"\\cf1 {KeyPair.Value.Replace("\\", "\\\\").Replace("\"", "&quot;")}\\cf3 \"");
                }
                Att = $" {String.Join(" ", attFormat)}";
            }
            Start = $"\\cf1 <\\cf2 {bxt.Name}{Att}\\cf1 {addition}>";
            if (OneLine)
            {
                formater.AddString(nowLevel, TextAlign.Top, $"{Start}\\cf3 {bxt.Text.Replace("\\", "\\\\").Replace("\"", "&quot;") ?? ""}{End}");
            }
            else
            {
                formater.AddString(nowLevel, TextAlign.Top, $"{Start}\\cf3 {bxt.Text.Replace("\\", "\\\\").Replace("\"", "&quot;") ?? ""}");
                foreach (BinaryXmlTag sub in bxt.Children)
                {
                    sub.ApplyToRichText(formater, nowLevel + 1);
                }
                formater.AddString(nowLevel, TextAlign.Top, End);
            }

        }
		
        public static void StylizeXmlToRichText(this string xml, RichTextBox richTextBox)
        {
            const string rtfHead = @"{\rtf1\ansi\ansicpg65001\deff0\nouicompat\deflang1033\deflangfe1028{\fonttbl{\f0\fnil Consolas;}}
{\colortbl ;\red0\green0\blue255;\red165\green42\blue42;\red0\green0\blue0;\red255\green0\blue0;\red0\green128\blue32;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1\fs18";
            
            XmlScanner scanner = new XmlScanner(new TextLineReader(xml));
            XmlStyleParser parser = new XmlStyleParser(scanner);

            var output = parser.Parse();

            StringBuilder stringBuilder = new StringBuilder();
            
            int currentIdx = 0;
            foreach (var item in output.OrderBy(x => (x.BeginPosition.Value)))
            {
	            switch (item)
	            {
		            case XmlStylizedDeclarationTag declarationTag:
			            MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagBeginPosition, 1);
			            MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagNamePosition, 2);
			            MarkAttributes(ref xml, stringBuilder, ref currentIdx, declarationTag.Attributes);
			            MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagEndPosition, 1);
			            break;
		            case XmlStylizedStartTag startTag:
			            MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagBeginPosition, 1);
			            MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagNamePosition, 2);
			            MarkAttributes(ref xml, stringBuilder, ref currentIdx, startTag.Attributes);
			            MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagEndPosition, 1);
			            break;
		            case XmlStylizedEndTag endTag:
			            MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagBeginPosition, 1);
			            MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagNamePosition, 2);
			            MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagEndPosition, 1);
			            break;
		            case XmlStylizedCommentTag commentTag:
			            MarkString(ref xml, stringBuilder, ref currentIdx, commentTag.CommentPosition, 5);
			            break;
	            }
            }

            if (currentIdx < xml.Length)
	            stringBuilder.Append(xml[currentIdx..]);

            // stringBuilder.Append("\r\n}");
            string rawStr = string.Join("\r\n", stringBuilder.ToString().Split("\r\n").Select(x => $"{EscapeNonAsciiChar(x)} \\par"));
            
            
            richTextBox.Rtf = $"{rtfHead}\r\n{rawStr}\r\n}}";
        }

	    private static void MarkAttributes(ref string refStr, StringBuilder stringBuilder, ref int currentIdx, IEnumerable<(Range key, Range? assign, Range? value)> attributes)
	    {
		    foreach (var attr in attributes)
		    {
			    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.key, 4);
			    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.assign, 3);
			    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.value, 1);
		    }
	    }
	    
	    private static void MarkString(ref string refStr, StringBuilder stringBuilder, ref int currentIdx, Range? range, int color)
	    {
		    if (range is null)
			    return;
		    if (range.Value.Start.Value > currentIdx)
			    stringBuilder.Append(refStr[currentIdx..range.Value.Start]);

		    stringBuilder.Append($"\\cf{color} ");
		    stringBuilder.Append(refStr[range.Value]); 
		    stringBuilder.Append($"\\cf0 ");

		    currentIdx = range.Value.End.Value;
	    }
	    
	    private static string EscapeNonAsciiChar(string input)
	    {
		    StringBuilder sb = new StringBuilder();
		    foreach (char c in input)
		    {
			    if (char.IsAscii(c))
			    {
				    sb.Append(c);
			    }
			    else
			    {
				    byte[] utf8_converted = Encoding.UTF8.GetBytes(new char[] { c });
				    foreach (byte b in utf8_converted)
				    {
					    sb.Append($@"\'{Convert.ToString(b, 16).PadLeft(2, '0')}");
				    }
			    }
		    }
		    return sb.ToString();
	    }
    }
}
