using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace eP.Text
{
    public class TextLineReader
    {
        private static readonly HashSet<char> blankChars = [' ', '　', '\t'];
        private static readonly HashSet<char> blankNewLineChars = [' ', '　', '\t', '\r', '\n'];
        
        public static IReadOnlySet<char> BlankCharacters => blankChars;
        
        // Members
        private string baseText;
        private int ln = 0;
        private int pos = 0;
        private int lineStartPos = 0;

        // Properies
        public string BaseText => baseText;
        public int Line => ln;
        public int Position => pos;
        public int PositionInLine => pos - lineStartPos;
        public bool IsBeginOfLine => PositionInLine == 0;
        public bool IsEnd => pos >= baseText.Length;
        public bool IsEndOfLine => IsEnd || newLineSymbolLen() > 0;

        // Constructors
        public TextLineReader(string text)
        {
            baseText = text;
        }

        // Public methods
        public char ReadChar()
        {
            if (IsEndOfLine) return '\0';
            else return baseText[pos++];
        }

        public string ReadString(int length)
        {
            if (IsEndOfLine) return "";
            int maxLen = (pos + length) > baseText.Length ? baseText.Length - pos : length;
            int startPos = pos;
            int outLen = 0;
            for(; outLen < maxLen && !IsEndOfLine; outLen++)
                pos++;
            return baseText.Substring(startPos, outLen);
        }

        public string ReadToLineEnd()
        {
            if (IsEnd)
                return "";
            int startPos = pos;
            while (!IsEndOfLine)
                pos++;
            return baseText.Substring(startPos, pos - startPos);
        }

        public bool IfChar(char ch, out char outCh)
        {
            outCh = '\0';
            if (IsEndOfLine)
                return false;
            if (baseText[pos] == ch)
            {
                outCh = baseText[pos];
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool IfCharSet(IReadOnlySet<char> allowCharSet, out char outCh)
        {
            outCh = '\0';
            if (IsEndOfLine)
                return false;
            if (allowCharSet.Contains(baseText[pos]))
            {
                outCh = baseText[pos];
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool IfNotCharSet(IReadOnlySet<char> notAllowCharSet)
        {
            if (IsEndOfLine)
                return false;
            if (!notAllowCharSet.Contains(baseText[pos]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool IfNotCharMask(bool[] notAllowCharMask)
        {
            if (IsEndOfLine)
                return false;
            char ch = baseText[pos];
            if (ch > 0xFF || !notAllowCharMask[ch])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool AcceptString(string compareStr)
        {
            if((pos + compareStr.Length) > baseText.Length) 
                return false;
            if(string.CompareOrdinal(baseText, pos, compareStr, 0, compareStr.Length) == 0)
            {
                pos += compareStr.Length;
                return true;
            }
            else 
                return false;
        }

        public bool AcceptString(string compareStr, out string outStr)
        {
            outStr = "";
            if((pos + compareStr.Length) > baseText.Length) 
                return false;
            if(string.CompareOrdinal(baseText, pos, compareStr, 0, compareStr.Length) == 0)
            {
                outStr = compareStr;
                pos += compareStr.Length;
                return true;
            }
            else 
                return false;
        }
        
        public bool AcceptChar(char compareCh)
        {
            if(IsEndOfLine)
                return false;
            if (baseText[pos] == compareCh)
            {
                pos++;
                return true; 
            }
            else
            {
                return false;
            }
        }

        public bool AcceptIf(IReadOnlySet<char> allowCharSet)
        {
            return AcceptIf(allowCharSet, out _);
        }
        
        public bool AcceptIf(IReadOnlySet<char> allowCharSet, out char outCh)
        {
            outCh = '\0';
            if (IsEndOfLine)
                return false;
            if (allowCharSet.Contains(baseText[pos]))
            {
                outCh = baseText[pos++];
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AcceptIfNot(IReadOnlySet<char> allowCharSet)
        {
            return AcceptIfNot(allowCharSet, out _);
        }
        
        public bool AcceptIfNot(IReadOnlySet<char> allowCharSet, out char outCh)
        {
            outCh = '\0';
            if (IsEndOfLine)
                return false;
            if (!allowCharSet.Contains(baseText[pos]))
            {
                outCh = baseText[pos++];
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ReadUntilChar(char terminalChar)
        {
            int startPos = pos;
            while(!IsEndOfLine) 
            {
                if (baseText[pos] == terminalChar)
                    break;
                else
                    pos++;
            }
            return baseText.Substring(startPos, pos - startPos);
        }
        
        public string ReadUntilCharMultipleLine(char terminalChar)
        {
            int startPos = pos;
            char prevCh = pos > 0 ? baseText[pos - 1] : '\0';
            while(!IsEnd)
            {
                char curCh = baseText[pos];
                if (baseText[pos] == terminalChar)
                    break;
                else
                    pos++;

                if (curCh is '\r' or '\n')
                {
                    if(curCh == '\n' && prevCh != '\r')
                        ln++;
                    lineStartPos = pos + 1;
                }

                prevCh = curCh;
            }
            return baseText.Substring(startPos, pos - startPos);
        }
        
        public string ReadUntil(IReadOnlySet<char> terminalCharSet)
        {
            int startPos = pos;
            while(!IsEndOfLine) 
            {
                if (terminalCharSet.Contains(baseText[pos]))
                    break;
                else
                    pos++;
            }
            return baseText.Substring(startPos, pos - startPos);
        }

        public string ReadIf(IReadOnlySet<char> allowCharSet)
        {
            int startPos = pos;
            while (!IsEndOfLine)
            {
                if (!allowCharSet.Contains(baseText[pos]))
                    break;
                else
                    pos++;
            }
            return baseText.Substring(startPos, pos - startPos);
        }
        
        public string ReadIfNot(IReadOnlySet<char> allowCharSet)
        {
            int startPos = pos;
            while (!IsEndOfLine)
            {
                if (allowCharSet.Contains(baseText[pos]))
                    break;
                else
                    pos++;
            }
            return baseText.Substring(startPos, pos - startPos);
        }
        
        public void SkipIf(IReadOnlySet<char> allowCharSet)
        {
            int startPos = pos;
            while (!IsEndOfLine)
            {
                if (!allowCharSet.Contains(baseText[pos]))
                    break;
                else
                    pos++;
            }
        }
        
        public void SkipIfNotCharSet(IReadOnlySet<char> allowCharSet)
        {
            while (!IsEndOfLine)
            {
                if (allowCharSet.Contains(baseText[pos]))
                    break;
                else
                    pos++;
            }
        }
        
        public void SkipIfNotCharMask(bool[] allowCharSet)
        {
            while (!IsEndOfLine)
            {
                char ch = baseText[pos];
                if (ch > 0xFF || !allowCharSet[ch])
                    break;
                else
                    pos++;
            }
        }
        
        public void SkipIfBlankMultipleLine()
        {
            char prevCh = pos > 0 ? baseText[pos - 1] : '\0';
            while (!IsEnd)
            {
                char curCh = baseText[pos];
                
                if (!blankNewLineChars.Contains(curCh))
                    break;
                else
                    pos++;
                
                if (curCh is '\r' or '\n')
                {
                    if(curCh == '\n' && prevCh != '\r')
                        ln++;
                    lineStartPos = pos + 1;
                }
                
                prevCh = curCh;
            }
        }
        
        public void SkipUntilCharMultipleLine(char terminalChar)
        {
            char prevCh = pos > 0 ? baseText[pos - 1] : '\0';
            while(!IsEnd)
            {
                char curCh = baseText[pos];
                if (curCh == terminalChar)
                    break;
                else
                    pos++;

                if (curCh is '\r' or '\n')
                {
                    if(curCh == '\n' && prevCh != '\r')
                        ln++;
                    lineStartPos = pos + 1;
                }

                prevCh = curCh;
            }
        }
        
        public char PeekChar()
        {
            if (IsEndOfLine)
                return '\0';
            else
                return baseText[pos];
        }

        public int CountIndentLevel(string indentStr)
        {
            int count = 0;
            while(AcceptString(indentStr))
                count++;
            return count;
        }

        public int CountIndentLevel(string indentStr, int maxCount)
        {
            int count = 0;
            while (count < maxCount && AcceptString(indentStr))
                count++;
            return count;
        }

        public bool MoveNextLine()
        {
            int newLineLen = 0;
            while (((newLineLen = newLineSymbolLen()) == 0) && !IsEnd)
                pos++;
            pos += newLineLen;
            lineStartPos = pos;
            if (!IsEnd)
                ln++;
            return !IsEnd;
        }

        // private methods
        private int newLineSymbolLen()
        {
            // If returns value is zero, it means current character is not new line symbol.
            if(IsEnd)
                return 0;
            char curCh = baseText[pos];
            char nextCh = (pos + 1) < baseText.Length ? baseText[pos + 1] : '\0';
            if (curCh is not '\r' and not '\n')
            {
                return 0;
            }
            else if (curCh is '\r')
            {
                if(nextCh is '\n')
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }
    }
}
