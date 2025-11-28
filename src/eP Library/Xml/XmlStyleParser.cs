using System.Runtime.CompilerServices;

namespace eP.Xml;

public class XmlStyleParser(XmlScanner scanner)
{
    public XmlScanner BaseScanner { get; } = scanner;
    
    public IReadOnlyList<XmlStylizedElement> Parse()
    {
        List<XmlStylizedElement> output = [];
        while (ParseElement(output)) ;

        return output;
    }

    public async Task<IReadOnlyList<XmlStylizedElement>> ParseAsync(CancellationToken token = default)
    {
        return await Task.Run(() =>
        {
            List<XmlStylizedElement> output = [];
            while (!token.IsCancellationRequested && ParseElement(output)) ;
            
            token.ThrowIfCancellationRequested();
            return output;
        }, token);
    }

    public bool ParseElement(List<XmlStylizedElement> output)
    {
        XmlToken startToken = new XmlToken();

        if (!BaseScanner.GetNextToken(ref startToken))
            return false;
        
        switch (startToken.Category)
        {
            case XmlTokenCategory.StartTagBegin:
                ParseStartTag(ref startToken, output);
                break;
            case XmlTokenCategory.EndTagBegin:
                ParseEndTag(ref startToken, output);
                break;
            case XmlTokenCategory.Comment:
                ParseComment(ref startToken, output);
                break;
            case XmlTokenCategory.DeclarationBegin:
                ParseXmlDeclaration(ref startToken, output);
                break;
            default:
                ParsePlanText(ref startToken, output);
                break;
        }

        return true;
    }

    public void ParseXmlDeclaration(ref XmlToken startToken, List<XmlStylizedElement> output)
    {
        List<(Range key, Range? assign, Range? value)> attrList = [];

        XmlToken identifierToken = new XmlToken();

        if (BaseScanner.GetNextToken(ref identifierToken) && identifierToken.Category != XmlTokenCategory.Identifier)
        {
            BaseScanner.PushBackToken(ref identifierToken);
            identifierToken.Invalidation();
        }
        
        XmlToken nextToken = new XmlToken();
        while (BaseScanner.GetNextToken(ref nextToken) && nextToken.Category != XmlTokenCategory.DeclarationEnd)
        {
            if (nextToken.Category == XmlTokenCategory.Identifier)
            {
                XmlToken assignToken = new XmlToken();
                XmlToken assignValue = new XmlToken(); 
                if (BaseScanner.GetNextToken(ref assignToken) && assignToken.Category is XmlTokenCategory.AttributeAssign)
                {
                    if (BaseScanner.GetNextToken(ref assignValue) && assignValue.Category is not XmlTokenCategory.QuotedIdentifier)
                    {
                        BaseScanner.PushBackToken(ref assignValue);
                        assignValue.Invalidation();
                    }
                }
                else if(assignToken.Category is not XmlTokenCategory.Invalid)
                {
                    BaseScanner.PushBackToken(ref assignToken);
                    assignToken.Invalidation();
                }

                (Range, Range?, Range?) attr = (
                    TokenToRangeNoNull(ref nextToken),
                    TokenToRange(ref assignToken),
                    TokenToRange(ref assignValue)
                );
                
                attrList.Add(attr);
            }
        }

        XmlStylizedElement outElement = new XmlStylizedDeclarationTag()
        {
            TagBeginPosition = TokenToRangeNoNull(ref startToken),
            TagNamePosition = TokenToRange(ref identifierToken),
            Attributes = attrList,
            TagEndPosition = TokenToRange(ref nextToken)
        };
        
        output.Add(outElement);
    }
    
    public void ParseStartTag(ref XmlToken startToken, List<XmlStylizedElement> output)
    {
        List<(Range key, Range? assign, Range? value)> attrList = [];

        XmlToken identifierToken = new XmlToken();

        if (BaseScanner.GetNextToken(ref identifierToken) && identifierToken.Category != XmlTokenCategory.Identifier)
        {
            BaseScanner.PushBackToken(ref identifierToken);
            identifierToken.Invalidation();
        }
        
        XmlToken nextToken = new XmlToken();
        while (BaseScanner.GetNextToken(ref nextToken) && nextToken.Category is not XmlTokenCategory.TagEnd and not XmlTokenCategory.StartTagEmptyEnd)
        {
            if (nextToken.Category == XmlTokenCategory.Identifier)
            {
                XmlToken assignToken = new XmlToken();
                XmlToken assignValue = new XmlToken(); 
                if (BaseScanner.GetNextToken(ref assignToken) && assignToken.Category is XmlTokenCategory.AttributeAssign)
                {
                    if (BaseScanner.GetNextToken(ref assignValue) && assignValue.Category is not XmlTokenCategory.QuotedIdentifier)
                    {
                        BaseScanner.PushBackToken(ref assignValue);
                        assignValue.Invalidation();
                    }
                }
                else if(assignToken.Category is not XmlTokenCategory.Invalid)
                {
                    BaseScanner.PushBackToken(ref assignToken);
                    assignToken.Invalidation();
                }

                (Range, Range?, Range?) attr = (
                    TokenToRangeNoNull(ref nextToken),
                    TokenToRange(ref assignToken),
                    TokenToRange(ref assignValue)
                );
                
                attrList.Add(attr);
            }
        }

        XmlStylizedElement outElement = new XmlStylizedStartTag()
        {
            TagBeginPosition = TokenToRangeNoNull(ref startToken),
            TagNamePosition = TokenToRange(ref identifierToken),
            Attributes = attrList,
            TagEndPosition = TokenToRange(ref nextToken)
        };
        
        output.Add(outElement);
    }
    
    public void ParseEndTag(ref XmlToken startToken, List<XmlStylizedElement> output)
    {
        XmlToken identifierToken = new XmlToken();

        if (BaseScanner.GetNextToken(ref identifierToken) && identifierToken.Category != XmlTokenCategory.Identifier)
        {
            BaseScanner.PushBackToken(ref identifierToken);
            identifierToken.Invalidation();
        }
        
        XmlToken nextToken = new XmlToken();
        while (BaseScanner.GetNextToken(ref nextToken) && nextToken.Category is not XmlTokenCategory.TagEnd) ;

        XmlStylizedElement outElement = new XmlStylizedEndTag()
        {
            TagBeginPosition = TokenToRangeNoNull(ref startToken),
            TagNamePosition = TokenToRange(ref identifierToken),
            TagEndPosition = TokenToRange(ref nextToken)
        };
        output.Add(outElement);
    }

    public void ParseComment(ref XmlToken startToken, List<XmlStylizedElement> output)
    {
        output.Add(new XmlStylizedCommentTag()
        {
            CommentPosition = TokenToRangeNoNull(ref startToken)
        });
    }
    
    public void ParsePlanText(ref XmlToken startToken, List<XmlStylizedElement> output)
    {
        XmlToken nextToken = startToken;
        Index begin = startToken.StartPosition;
        Index end = nextToken.EndPosition;
        while (BaseScanner.GetNextToken(ref nextToken) && 
               nextToken.Category is 
                   not XmlTokenCategory.DeclarationBegin and 
                   not XmlTokenCategory.StartTagBegin and 
                   not XmlTokenCategory.EndTagBegin and
                   not XmlTokenCategory.Comment)
        {
            end = nextToken.EndPosition;
        }
        
        output.Add(new XmlStylizedPlanText()
        {
            PlanTextPosition = new Range(begin, end)
        });
        
        if(nextToken.Category is not XmlTokenCategory.Invalid)
            BaseScanner.PushBackToken(ref nextToken);
    }

    public static Range? TokenToRange(ref XmlToken token)
    {
        if (token.Category is XmlTokenCategory.Invalid)
            return null;

        return new Range(token.StartPosition, token.EndPosition);
    }
    
    public static Range TokenToRangeNoNull(ref XmlToken token)
    {
        return new Range(token.StartPosition, token.EndPosition);
    }
}