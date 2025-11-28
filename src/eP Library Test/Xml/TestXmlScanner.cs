using System.Diagnostics;
using System.Text;
using eP.Command;
using eP.Testing;
using eP.Text;
using eP.Xml;

namespace ePLibraryTest.Xml;

public class TestXmlScanner: TestStage
{
	private string _testXml = "";
	
    public TestXmlScanner()
    {
	    AddStep("ScannerTest", () =>
	    {
		    int count = 10;
		    XmlToken xmlToken = new XmlToken();
		    while (count-- > 0)
		    {
			    XmlScanner scanner = new XmlScanner(new TextLineReader(_testXml));

			    while (scanner.GetNextToken(ref xmlToken)) ;
		    }
	    });
        AddStep("ScannerParserTest", () =>
        {
	        int count = 10;
	        while (count-- > 0)
	        {
		        XmlScanner scanner = new XmlScanner(new TextLineReader(_testXml));
		        XmlStyleParser parser = new XmlStyleParser(scanner);

		        var output = parser.Parse();
	        }
        });
    }

    [Command("setfile")]
    private CommandExecuteResult SetFile(IConsole console, CommandArgumentQueue argumentQueue)
    {
	    string fileName = argumentQueue.PopArgumentString();
	    if (!File.Exists(fileName))
		    return new CommandExecuteResult(ResultType.Failure, $"Can't find file: {fileName}");

	    using var stream = new FileStream(fileName, FileMode.Open);
	    StreamReader reader = new StreamReader(stream);
	    _testXml = reader.ReadToEnd();

	    return new CommandExecuteResult(ResultType.Success, "");
    }
		
    [Command("test1")]
    private CommandExecuteResult Test1(IConsole console, CommandArgumentQueue argumentQueue)
    {
	    TestFunction(console, _testXml);

	    return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("test2")]
    private CommandExecuteResult Test2(IConsole console, CommandArgumentQueue argumentQueue)
    {
	    TestFunction2(console, _testXml);

	    return new CommandExecuteResult(ResultType.Success, "");
    }

    private void TestFunction(IConsole console, string xml)
    {
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
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagBeginPosition, 34);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagNamePosition, 36);
			        MarkAttributes(ref xml, stringBuilder, ref currentIdx, declarationTag.Attributes);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagEndPosition, 34);
			        break;
		        case XmlStylizedStartTag startTag:
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagBeginPosition, 36);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagNamePosition, 36);
			        MarkAttributes(ref xml, stringBuilder, ref currentIdx, startTag.Attributes);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagEndPosition, 36);
			        break;
		        case XmlStylizedEndTag endTag:
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagBeginPosition, 36);
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagNamePosition, 36);
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagEndPosition, 36);
			        break;
		        case XmlStylizedCommentTag commentTag:
			        MarkString(ref xml, stringBuilder, ref currentIdx, commentTag.CommentPosition, 32);
			        break;
	        }
        }

        if (currentIdx < xml.Length)
	        stringBuilder.Append(xml[currentIdx..]);
        
        console.WriteLine(stringBuilder.ToString());
    }
    
    private void TestFunction2(IConsole console, string xml)
    {
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
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagBeginPosition, 34);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagNamePosition, 36);
			        MarkAttributes(ref xml, stringBuilder, ref currentIdx, declarationTag.Attributes);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, declarationTag.TagEndPosition, 34);
			        break;
		        case XmlStylizedStartTag startTag:
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagBeginPosition, 36);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagNamePosition, 36);
			        MarkAttributes(ref xml, stringBuilder, ref currentIdx, startTag.Attributes);
			        MarkString    (ref xml, stringBuilder, ref currentIdx, startTag.TagEndPosition, 36);
			        break;
		        case XmlStylizedEndTag endTag:
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagBeginPosition, 36);
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagNamePosition, 36);
			        MarkString(ref xml, stringBuilder, ref currentIdx, endTag.TagEndPosition, 36);
			        break;
		        case XmlStylizedCommentTag commentTag:
			        MarkString(ref xml, stringBuilder, ref currentIdx, commentTag.CommentPosition, 32);
			        break;
	        }
        }

        if (currentIdx < xml.Length)
	        stringBuilder.Append(xml[currentIdx..]);
        
        console.WriteLine(stringBuilder.ToString());
    }

    private void MarkAttributes(ref string refStr, StringBuilder stringBuilder, ref int currentIdx, IEnumerable<(Range key, Range? assign, Range? value)> attributes)
    {
	    foreach (var attr in attributes)
	    {
		    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.key, 31);
		    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.assign, 33);
		    MarkString(ref refStr, stringBuilder, ref currentIdx, attr.value, 35);
	    }
    }
    
    private void MarkString(ref string refStr, StringBuilder stringBuilder, ref int currentIdx, Range? range, int color)
    {
	    if (range is null)
		    return;
	    if (range.Value.Start.Value > currentIdx)
		    stringBuilder.Append(refStr[currentIdx..range.Value.Start]);

	    stringBuilder.Append($"\u001b[{color}m");
	    stringBuilder.Append(refStr[range.Value]); 
	    stringBuilder.Append($"\u001b[0m");

	    currentIdx = range.Value.End.Value;
    }
}

