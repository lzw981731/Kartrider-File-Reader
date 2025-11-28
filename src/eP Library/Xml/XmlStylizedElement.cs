namespace eP.Xml;

public abstract class XmlStylizedElement
{
    public abstract XmlStylizedCategory Category { get;  }
    public abstract Index BeginPosition { get; }
}

public class XmlStylizedStartTag: XmlStylizedElement
{
    public override XmlStylizedCategory Category => XmlStylizedCategory.StartTag;

    public override Index BeginPosition => TagBeginPosition.Start;

    public Range TagBeginPosition { get; set; }
    public Range? TagNamePosition { get; set; }
    public List<(Range key, Range? assign, Range? value)> Attributes { get; set; } = [];
    public Range? TagEndPosition { get; set; }
}

public class XmlStylizedDeclarationTag: XmlStylizedElement
{
    public override XmlStylizedCategory Category => XmlStylizedCategory.XmlDeclaration;
    
    public override Index BeginPosition => TagBeginPosition.Start;
    
    public Range TagBeginPosition { get; set; }
    public Range? TagNamePosition { get; set; }
    public List<(Range key, Range? assign, Range? value)> Attributes { get; set; } = [];
    public Range? TagEndPosition { get; set; }
}

public class XmlStylizedEndTag: XmlStylizedElement
{
    public override XmlStylizedCategory Category => XmlStylizedCategory.StartTag;
    
    public override Index BeginPosition => TagBeginPosition.Start;
    
    public Range TagBeginPosition { get; set; }
    public Range? TagNamePosition { get; set; }
    public Range? TagEndPosition { get; set; }
}

public class XmlStylizedCommentTag : XmlStylizedElement
{
    public override XmlStylizedCategory Category => XmlStylizedCategory.Comment;
    
    public override Index BeginPosition => CommentPosition.Start;
    public Range CommentPosition { get; set; }
}

public class XmlStylizedPlanText : XmlStylizedElement
{
    public override XmlStylizedCategory Category => XmlStylizedCategory.PlanText;
    
    public override Index BeginPosition => PlanTextPosition.Start;
    
    public Range PlanTextPosition { get; set; }
}