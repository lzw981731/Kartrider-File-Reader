// #define SCANNER_USE_RAWSTRING

using System.Runtime.CompilerServices;
using System.Text;
using eP.Text;

namespace eP.Xml;


public class XmlScanner(TextLineReader baseReader)
{
    public TextLineReader BaseReader { get; set; } = baseReader;

    private Stack<XmlToken> _tokenStack = [];

    private static readonly HashSet<char> IdentifierNotSet = [
        .."!\"#$%&'()*+,/;<=>?",
        ..TextLineReader.BlankCharacters
    ];
    
    public void PushBackToken(ref XmlToken token)
    {
        _tokenStack.Push(token);
    }

    public bool GetNextToken(ref XmlToken output)
    {
        if (_tokenStack.Count > 0)
        {
            output = _tokenStack.Pop();
            return true;
        }

        while (!BaseReader.IsEnd)
        {
            BaseReader.SkipIfBlankMultipleLine();

            if (BaseReader.AcceptChar('<'))
            {
                if (BaseReader.AcceptChar('?'))
                {
                    output = new XmlToken()
                    {
                        Category = XmlTokenCategory.DeclarationBegin,
                        StartPosition = BaseReader.Position - 2,
                        EndPosition = BaseReader.Position,
                    };
                    
                    return true;
                }
                else if (BaseReader.AcceptChar('/'))
                {
                    output = new XmlToken()
                    {
                        Category = XmlTokenCategory.EndTagBegin,
                        StartPosition = BaseReader.Position - 2,
                        EndPosition = BaseReader.Position,
                    };

                    return true;
                }
                else if (BaseReader.AcceptString("!--"))
                {
                    output = new XmlToken()
                    {
                        Category = XmlTokenCategory.Comment,
                        StartPosition = BaseReader.Position - 4,
                    };

                    while (!BaseReader.IsEnd)
                    {
                        BaseReader.SkipUntilCharMultipleLine('-');

                        if (BaseReader.AcceptString("-->"))
                        {
                            break;
                        }
                        else
                        {
                            BaseReader.AcceptChar('-');
                        }
                    }

                    // If there is no "-->", comment will extend to the end of string.
                    output.EndPosition = BaseReader.Position;

                    return true;
                }
                else
                {
                    output = new XmlToken()
                    {
                        Category = XmlTokenCategory.StartTagBegin,
                        StartPosition = BaseReader.Position - 1,
                        EndPosition = BaseReader.Position,
                    };
                    
                    return true;
                }
            }
            else if (BaseReader.AcceptString("?>"))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.DeclarationEnd,
                    StartPosition = BaseReader.Position - 2,
                    EndPosition = BaseReader.Position,
                };
                
                return true;
            }
            else if (BaseReader.AcceptString("/>"))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.StartTagEmptyEnd,
                    StartPosition = BaseReader.Position - 2,
                    EndPosition = BaseReader.Position,
                };
                
                return true;
            }
            else if (BaseReader.AcceptChar('>'))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.TagEnd,
                    StartPosition = BaseReader.Position - 1,
                    EndPosition = BaseReader.Position,
                };
                
                return true;
            }
            else if (BaseReader.AcceptChar('"'))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.QuotedIdentifier,
                    StartPosition = BaseReader.Position - 1
                };

                BaseReader.SkipUntilCharMultipleLine('"');
                BaseReader.AcceptChar('"');

                output.EndPosition = BaseReader.Position;
                
                return true;
            }
            else if (BaseReader.AcceptChar('\''))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.QuotedIdentifier,
                    StartPosition = BaseReader.Position - 1
                };

                BaseReader.SkipUntilCharMultipleLine('\'');
                BaseReader.AcceptChar('\'');

                output.EndPosition = BaseReader.Position;

                return true;
            }
            else if (BaseReader.AcceptChar('='))
            {
                output =  new XmlToken()
                {
                    Category = XmlTokenCategory.AttributeAssign,
                    StartPosition = BaseReader.Position - 1,
                    EndPosition = BaseReader.Position,
                };
                
                return true;
            }
            else if (BaseReader.IfNotCharSet(IdentifierNotSet))
            {
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.Identifier,
                    StartPosition = BaseReader.Position
                };
                
                BaseReader.SkipIfNotCharSet(IdentifierNotSet);


                output.EndPosition = BaseReader.Position;

                return true;
            }
            else
            {
                char unCh = BaseReader.ReadChar();
                output = new XmlToken()
                {
                    Category = XmlTokenCategory.Unknown,
                    StartPosition = BaseReader.Position - 1,
                    EndPosition = BaseReader.Position - 1,
#if SCANNER_USE_RAWSTRING
                    RawString = "" + unCh,
#endif
                };
                
                return true;
            }
        }

        output.Category = XmlTokenCategory.Invalid;
        return false;
    }
}