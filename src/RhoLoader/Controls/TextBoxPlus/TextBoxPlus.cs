using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using eP.Extension;

namespace RhoLoader.Controls.TextBoxPlus;

public partial class TextBoxPlus : UserControl
{
    private int _hMoveUnit = 11;
    private bool _textUpdateReqired = true;
    private string _newLineSymbol = Environment.NewLine;
    private ConcurrentDictionary<char, float> _chWidthCache = [];
    private string _text = "";
    private List<string> _lines = [];
    private List<int> _linesPosition = [];
    private List<TextBoxPlusTextColor> _foreColors = [];
    private List<TextBoxPlusTextColor> _backColors = [];
    
    private CancellationTokenSource? _cancellationTokenSource;

    private int _linePosBufferTopLineIdx = -1;
    private float[][] _linePosBuffer = [];

    private bool _cursorStatus = true; // true: show, false: hide.
    
    private ScrollBarNativeListener _scrollBarNativeListener;

    private int _textCursorStart = 0;
    private int _textCursorEnd = 0;
    private float _textCursorX = 0;

    private float _maxWidth = 0;
    
    public override string Text
    {
        get => _text;
        set => UpdateText(value);
    }
    
    public TextBoxPlus()
    {
        InitializeComponent();

        _scrollBarNativeListener = new ScrollBarNativeListener(_vScrollBar);
        _scrollBarNativeListener.MouseWheel += ActionMouseWheel;
        this.MouseWheel += ActionMouseWheel;
    }

    public void InitForeColor(IEnumerable<TextBoxPlusTextColor> colors, bool ordered = false)
    {
        TextBoxPlusTextColor[] colorArray = colors.ToArray();
        if(!ordered)
            Array.Sort(colorArray, (color1, color2) => color1.From.CompareTo(color2.From));
        
        int i = 0;
        TextBoxPlusTextColor previousColor = new TextBoxPlusTextColor();
        
        foreach (var color in colorArray)
        {
            if (color.From >= color.To)
                throw new ArgumentException($"Text range: from >= to.");
            
            if (i == 0)
            {
                if (color.From < 0)
                    throw new ArgumentException("Text range: from is negative.");
            }
            else
            {
                if (color.From < previousColor.To)
                    throw new ArgumentException("This text range is intersect with other.");
            }

            previousColor = color;
            i++;
        }

        if (this.InvokeRequired)
        {
            this.Invoke(() =>
            {
                _foreColors.Clear();
                _foreColors.AddRange(colorArray);
            });
        }
        else
        {
            _foreColors.Clear();
            _foreColors.AddRange(colorArray);
        }
        
        Invalidate();
    }

    public string? GetSelectedText()
    {
        if (_textCursorStart == _textCursorEnd)
            return null;

        int textSelectBegin = Math.Clamp(Math.Min(_textCursorStart, _textCursorEnd), 0, _text.Length);
        int textSelectEnd = Math.Clamp(Math.Max(_textCursorStart, _textCursorEnd), 0, _text.Length);
        return _text.Substring(textSelectBegin, textSelectEnd - textSelectBegin);
    }
    
    private void UpdateText(string text)
    {
        string[] lines = text.Split(_newLineSymbol);
        List<string> newLines = [..lines];
        List<int> newPositions = [];
        newPositions.EnsureCapacity(lines.Length + 1);
        newPositions.Add(0);
        
        int previousPos = 0;
        for (int i = 1; i <= lines.Length; i++)
        {
            int pos = previousPos + lines[i - 1].Length + 2;
            newPositions.Add(pos);
            previousPos = pos;
        }

        Action invokeReqAction = () =>
        {
            _text = text;
            _lines.Clear();
            _linesPosition.Clear();
            _lines = newLines;
            _linesPosition = newPositions;
            _cursorFlicker.Start();
            _vScrollBar.Maximum = Math.Max(_lines.Count - 1, 0) + _vScrollBar.LargeChange - 1;
            _textUpdateReqired = true;

            if (IsHandleCreated)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => MeasureMaxWidthWorker(CreateGraphics(), _cancellationTokenSource.Token));
            }
        };

        if (this.InvokeRequired)
            this.Invoke(invokeReqAction);
        else
            invokeReqAction.Invoke();
        
        Invalidate();
    }

    private void ActionLoad(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
            
        Task.Run(() => MeasureMaxWidthWorker(CreateGraphics(), _cancellationTokenSource.Token));
    }
    
    private void ActionPaint(object sender, PaintEventArgs e)
    {
        int from = _vScrollBar.Value;
        int lineHeight = Font.Height;
        int to = Math.Min(from + GetDisplayLines(), _lines.Count);
        TextBoxPlusTextColor dummy = new TextBoxPlusTextColor();
        
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias; 
        
        SizeF spaceSize = e.Graphics.MeasureString("A A", Font) - e.Graphics.MeasureString("AA", Font);
        SizeF tabSize = e.Graphics.MeasureString("\t", Font);

        if (from < to)
        {
            UpdateLineChWidthBuffer(e.Graphics, from);
            
            dummy.From = _linesPosition[from];
            
            var beginIdx = _foreColors.AdvanceBinSearch(
                dummy, 
                (x, y) => x.From.CompareTo(y.From), 
                BinarySearchMode.LessThanOrEqual
            );
            
            // for all x, x.From > dummy.From 
            if (beginIdx < 0)
            {
                beginIdx = 0;
            }
            
            int xOffset = _hScrollBar.Value * _hMoveUnit;
            
            for (int i = from; i < to; i++)
            {
                string lineText = _lines[i];
                dummy.From = _linesPosition[i];
                dummy.To = _linesPosition[i + 1] - 2;

                float[] linePosBuf = _linePosBuffer[i - from];

                int currentPos = dummy.From;

                float x = 0, y = (i - from) * lineHeight;
                
                // e.Graphics.DrawString(lineText, Font, Brushes.Black, 0, (i - from) * lineHeight);
                
                if (_textCursorStart != _textCursorEnd)
                {
                    int cursorBegin = Math.Min(_textCursorStart, _textCursorEnd);
                    int cursorEnd = Math.Max(_textCursorStart, _textCursorEnd);
                    if (cursorBegin <= dummy.To && cursorEnd >= dummy.From)
                    {
                        cursorBegin = Math.Max(cursorBegin, dummy.From);
                        cursorEnd = Math.Min(cursorEnd, dummy.To);

                        float selectBeginX = Math.Max(linePosBuf[cursorBegin - dummy.From] + 1 - xOffset, 0);
                        float selectEndX = Math.Min(linePosBuf[cursorEnd - dummy.From] + 1 - xOffset, Width);

                        if (Math.Max(_textCursorStart, _textCursorEnd) > dummy.To)
                            selectEndX += spaceSize.Width;
                        
                        if(selectEndX >= 0)
                            e.Graphics.FillRectangle(new SolidBrush(Color.Silver), selectBeginX, (i - from) * lineHeight, selectEndX - selectBeginX, lineHeight);
                    }
                }
                
                while (beginIdx < _foreColors.Count && currentPos < dummy.To)
                {
                    TextBoxPlusTextColor textColor = _foreColors[beginIdx];
                    int textColorFrom = Math.Max(textColor.From, dummy.From);
                    int textColorTo = Math.Min(textColor.To, dummy.To);
                    if (textColor.To <= currentPos)
                    {
                        beginIdx++;
                        continue;
                    }

                    if (textColorFrom >= dummy.To)
                        break;

                    if (textColorFrom > currentPos)
                    {
                        DrawString(e.Graphics, ref lineText, _linePosBuffer[i - from], currentPos - dummy.From, textColorFrom - dummy.From, Color.Black, ref x, ref y);
                        currentPos = textColorFrom;
                    }
                    
                    DrawString(e.Graphics, ref lineText, _linePosBuffer[i - from], currentPos - dummy.From, textColorTo - dummy.From, textColor.Color, ref x, ref y);
                    
                    currentPos = textColorTo;
                }

                if (currentPos < dummy.To)
                {
                    DrawString(e.Graphics, ref lineText, _linePosBuffer[i - from], currentPos - dummy.From, dummy.To - dummy.From, Color.Black, ref x, ref y);
                }
                
                if (_textCursorEnd >= dummy.From && _textCursorEnd <= dummy.To)
                {
                    float cursorX = linePosBuf[_textCursorEnd - dummy.From] + 2 - xOffset;
                    if(_cursorStatus)
                        e.Graphics.DrawLine(new Pen(Color.Black, 2), cursorX, (i - from) * lineHeight, cursorX, (i - from + 1) * lineHeight);
                }
            }
        }
        
        e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault; 
    }

    private void DrawString(Graphics graphics, ref string text, float[] charPosBuf,int from, int to,Color color, ref float x, ref float y)
    {
        string subText = text.Substring(from, to - from);
        // string orgText = $"{subText}A";
        // string repText = "A";
        
        // StringFormat format = new StringFormat();
        // format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
        // format.Trimming = StringTrimming.None;
        // format.SetMeasurableCharacterRanges([new CharacterRange(from, to - from)]);
        //
        // Size dummySize = new Size(100000, 100000);
        //
        // TextFormatFlags flags = TextFormatFlags.NoClipping | TextFormatFlags.ExpandTabs | TextFormatFlags.TextBoxControl;
        
        // if (subText.StartsWith(' '))
        //     x += spaceSize.Width;
        
        //var bound = (TextRenderer.MeasureText(subText + subText, Font, dummySize, flags) - TextRenderer.MeasureText(subText, Font, dummySize, flags));
        // var bound = graphics.MeasureString(orgText, Font) - graphics.MeasureString(repText, Font);
        int i = from;
        int xOffset = _hScrollBar.Value * _hMoveUnit;
        foreach(var ch in subText)
        {
            x = charPosBuf[i++];
            float nextX = charPosBuf[i];
            if(nextX >= xOffset)
                graphics.DrawString("" + ch, Font, new SolidBrush(color), x - xOffset, y);
        }

        // if (subText.EndsWith(' '))
        //     x += spaceSize.Width;
        // x += bound.Width;
    }
    
    private void ActionVScroll(object sender, ScrollEventArgs e)
    {
        
    }

    private void ActionVScrollValueChanged(object sender, EventArgs e)
    {
        Refresh();
    }
    
    private void ActionHScrollValueChanged(object sender, EventArgs e)
    {
        Refresh();
    }

    private void ActionMouseWheel(object? sender, MouseEventArgs e)
    {
        _vScrollBar.Value = Math.Clamp(_vScrollBar.Value + e.Delta * -3 / 120, 0, _lines.Count - 1);
    }
    
    private int MousePointToTextPos(Point point, out float adjustX)
    {
        adjustX = point.X;
        int xOffset = _hScrollBar.Value * _hMoveUnit;
        
        int lineHeight = Font.Height;
        int absoluteLine = Math.Clamp(Math.Max(point.Y, 0) / lineHeight + _vScrollBar.Value, 0, _lines.Count - 1);
        int relativeLine = absoluteLine - _vScrollBar.Value;
        string lineBuf = _lines[absoluteLine];
        
        if (_linePosBuffer.Length > relativeLine)
        {
            float[] charPosBuf = _linePosBuffer[relativeLine];
            int column = MapXToColumn(charPosBuf, Math.Max(point.X + xOffset, 0));
            adjustX = charPosBuf[column];
            return _linesPosition[absoluteLine] + column;
        }
        
        return -1;
    }

    private int MapXToColumn(float[] lineChWidthBuf, float xPos)
    {
        int column = lineChWidthBuf.AdvanceBinSearch(Math.Max(xPos, 0), (x, y) => x.CompareTo(y), BinarySearchMode.LessThanOrEqual);
        
        if (column < lineChWidthBuf.Length - 1)
        {
            float widthC1 = lineChWidthBuf[column];
            float widthC2 = lineChWidthBuf[column + 1];

            if ((widthC2 - widthC1) >= 0.5)
            {
                column +=  (int)Math.Round((xPos - widthC1) / (widthC2 - widthC1));
            }
        }
        return column;
    }

    private float[] GetCharacterPosArray(Graphics graphics, ref string lineStr)
    {
        float[] outArray = new float[lineStr.Length + 1];
        int i = 1;

        float prevPos = 0;
        foreach(var ch in lineStr)
        {
            float width = _chWidthCache.GetOrAdd(ch, (key) =>
            {
                string orgText = $"{key}A";
                string repText = "A";

                SizeF chSize = graphics.MeasureString(orgText, Font) - graphics.MeasureString(repText, Font);
                return chSize.Width;
            });

            outArray[i++] = prevPos += width;
        }

        return outArray;
    }
    
    private void MeasureMaxWidthWorker(Graphics graphics, CancellationToken cancelToken)
    {
        float output = 0;
        foreach(var line in _lines)
        {
            if (cancelToken.IsCancellationRequested)
                break;
            string lineRef = line;
            var chSize = GetCharacterPosArray(graphics, ref lineRef);
            if(chSize.Length > 0)
                output = MathF.Max(output, chSize[^1]);
        }
        
        if(!cancelToken.IsCancellationRequested)
            UpdateMaxWidth(output);
    }
    
    private void UpdateMaxWidth(float maxWidth)
    {
        if (this.InvokeRequired)
            this.Invoke(UpdateMaxWidth, maxWidth);
        else
        {
            _maxWidth = (int)Math.Ceiling(maxWidth);
            
            UpdateHScrollMaximum();
        }
    }

    private void ActionResize(object sender, EventArgs e)
    {
        UpdateHScrollMaximum();
    }
    
    private void ActionMouseClick(object sender, MouseEventArgs e)
    {
        
    }

    private void ActionCursorFlick(object sender, EventArgs e)
    {
        _textUpdateReqired = true;
        _cursorStatus ^= true;
        
        Refresh();
    }

    private void ActionMouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button.HasFlag(MouseButtons.Left))
        {
            int newCursorPos = MousePointToTextPos(e.Location, out float adjustX);
            if (newCursorPos >= 0)
            {
                _textCursorStart = _textCursorEnd = newCursorPos;
                _textCursorX = adjustX;
                _cursorStatus = true;
                
                Refresh();
            }
        }
        else if (e.Button.HasFlag(MouseButtons.Right))
        {
            _contextMenu.Show((Control)sender, e.Location);
        }
    }

    private void ActionMouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button.HasFlag(MouseButtons.Left))
        {
            int newCursorPos = MousePointToTextPos(e.Location, out float adjustX);
            if (newCursorPos >= 0)
            {
                _textCursorEnd = newCursorPos;
                _textCursorX = adjustX;
                _cursorStatus = true;
            
                Refresh();
            }
        }
    }

    private void ActionKeyPress(object sender, KeyPressEventArgs e)
    {
        
    }

    private void ActionKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        if (e.Control)
        {
            if (e.KeyCode == Keys.C)
            {
                Copy();
            }
            else if (e.KeyCode == Keys.A)
            {
                SelectAll();
            }
        }

        if (e.KeyCode is Keys.Up or Keys.Down)
        {
            HandleKeyUpDown(CreateGraphics(), e.KeyData);
        }
        else if (e.KeyCode is Keys.Left or Keys.Right)
        {
            HandleKeyLeftRight(CreateGraphics(), e.KeyData);
        }
    }

    private void ActionPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        e.IsInputKey = false;
    }

    private void ActionCopyClick(object sender, EventArgs e)
    {
        Copy();
    }

    private void ActionSelectAllClick(object sender, EventArgs e)
    {
        SelectAll();
    }

    private void Copy()
    {
        string? selectedText = GetSelectedText();
        if (selectedText is not null)
        {
            Clipboard.SetText(selectedText);
        }
    }
    
    private void SelectAll()
    {
        (_textCursorStart, _textCursorEnd) = (0, _text.Length);
        
        Refresh();
    }

    private int GetTextBoxWidth()
    {
        int output = Width;
        if (_vScrollBar.Visible)
            output = Math.Max(output - _vScrollBar.Width, 0);
        return output;
    }

    private void UpdateHScrollMaximum()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(UpdateHScrollMaximum);
        }
        else
        {
            _hScrollBar.Maximum = (int)Math.Ceiling(Math.Max(_maxWidth - (GetTextBoxWidth()), 0) / _hMoveUnit);
            _hScrollBar.Visible = _hScrollBar.Maximum > 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cursorPos"></param>
    /// <returns>
    /// Line: range[0, _lines.Count)
    /// Column: 
    /// </returns>
    private (int, int) CursorToLineColumn(int cursorPos)
    {
        if (_linesPosition.Count == 0)
            return (0, 0);
        
        // lineIdx range: [-1, _lines.Count)
        int lineIdx = _linesPosition.AdvanceBinSearch(cursorPos, (x, y) => x.CompareTo(y), BinarySearchMode.LessThanOrEqual);
        if (lineIdx < 0)
            lineIdx = 0;
        
        // lineIdx range: [0, _lines.Count)

        int columnPosition = cursorPos - _linesPosition[lineIdx];
        return (lineIdx, columnPosition);
    }

    private void HandleKeyUpDown(Graphics graphics, Keys key)
    {
        if (_lines.Count == 0)
            return;

        int curTopLineIdx = _vScrollBar.Value;
        int curEndLineIdx = curTopLineIdx + _linePosBuffer.Length - 1;
        
        (int line, int column) pos = CursorToLineColumn(_textCursorEnd);

        string curLine = _lines[pos.line];
        float cursorX = GetCharacterPosArray(graphics, ref curLine)[pos.column];
        
        if ((key & Keys.KeyCode) == Keys.Up)
            pos.line = Math.Max(0, pos.line - 1);
        else if((key & Keys.KeyCode) == Keys.Down)
            pos.line = Math.Min(_linesPosition.Count - 1, pos.line + 1);

        if (curEndLineIdx <= pos.line || curTopLineIdx > pos.line)
        {
            if (curTopLineIdx > pos.line)
            {
                 VScrollTo(graphics, pos.line);
            }
            else
            {
                VScrollTo(graphics, pos.line - GetDisplayLines() + 2);
            }

            curTopLineIdx = _vScrollBar.Value;
        }

        pos.column = MapXToColumn(_linePosBuffer[pos.line - curTopLineIdx], _textCursorX);
         
        _textCursorEnd = _linesPosition[pos.line] + pos.column;

        if (!key.HasFlag(Keys.Shift))
            _textCursorStart = _textCursorEnd;
        
        _cursorStatus = true;
        Refresh();
    }
    
    private void HandleKeyLeftRight(Graphics graphics, Keys key)
    {
        if (_lines.Count == 0)
            return;

        int curTopLineIdx = _vScrollBar.Value;
        int curEndLineIdx = curTopLineIdx + _linePosBuffer.Length - 1;
        int curLeftX = _hScrollBar.Value * _hMoveUnit;
        int curRightX = curLeftX + GetTextBoxWidth();
        
        (int line, int column) pos = CursorToLineColumn(_textCursorEnd);
        
        if ((key & Keys.KeyCode) == Keys.Left)
        {
            if (pos.column > 0)
            {
                pos.column--;
            }
            else if (pos.line > 0)
            {
                pos.line--;
                pos.column = _lines[pos.line].Length;
            }
        }
        else if((key & Keys.KeyCode) == Keys.Right)
        {
            if (pos.column < _lines[pos.line].Length)
                pos.column++;
            else if (pos.line < (_lines.Count - 1))
            {
                pos.line++;
                pos.column = 0;
            }
        }

        if (curEndLineIdx <= pos.line || curTopLineIdx > pos.line)
        {
            if (curTopLineIdx > pos.line)
            {
                VScrollTo(graphics, pos.line);
            }
            else
            {
                VScrollTo(graphics, pos.line - GetDisplayLines() + 2);
            }

            curTopLineIdx = _vScrollBar.Value;
        }

        float cursorX = _linePosBuffer[pos.line - curTopLineIdx][pos.column];
        if (curRightX < cursorX || curLeftX > cursorX)
        {
            if (curRightX < cursorX)
            {
                _hScrollBar.Value = (int)
                    Math.Ceiling(Math.Clamp((cursorX - GetTextBoxWidth()) / _hMoveUnit, 0, _hScrollBar.Maximum));
            }
            else
            {
                _hScrollBar.Value = (int)
                    Math.Ceiling(Math.Clamp((cursorX - _hMoveUnit) / _hMoveUnit, 0, _hScrollBar.Maximum));
            }
            Refresh();
        }
        
        _textCursorEnd = _linesPosition[pos.line] + pos.column;
        _textCursorX = cursorX;

        if (!key.HasFlag(Keys.Shift))
            _textCursorStart = _textCursorEnd;
        
        _cursorStatus = true;
        Refresh();
    }

    private int GetDisplayLines()
    {
        int lineHeight = Font.Height;

        return Math.Max((Height + lineHeight - 1) / lineHeight, 1);
    }

    private void VScrollTo(Graphics graphics, int lineIdx)
    {
        if (_lines.Count == 0)
            return;
        
        _vScrollBar.Value = Math.Clamp(lineIdx, 0, _lines.Count - 1);
        UpdateLineChWidthBuffer(graphics, _vScrollBar.Value);
    }
    
    private void UpdateLineChWidthBuffer(Graphics graphics, int topIdx)
    {
        if (_lines.Count == 0)
            return;
        
        topIdx = Math.Min(topIdx, _lines.Count - 1);
        int endIdx = Math.Min(topIdx + GetDisplayLines(), _lines.Count);
        int newCount = endIdx - topIdx;
        if (_linePosBufferTopLineIdx == topIdx)
        {
            if (newCount != _linePosBuffer.Length)
            {
                float[][] oldBuf = _linePosBuffer;
                _linePosBuffer = new float[newCount][];
                Array.Copy(oldBuf, _linePosBuffer, Math.Min(newCount, oldBuf.Length));
                for (int i = oldBuf.Length; i < newCount; i++)
                {
                    string str = _lines[i + topIdx];
                    _linePosBuffer[i] = GetCharacterPosArray(graphics, ref str);
                }
            }
            
            return;
        }

        if (topIdx < 0)
            throw new Exception("topIdx cannot less than zero.");
        

        _linePosBuffer = new float[endIdx - topIdx][];
        
        
        for (int i = topIdx; i < endIdx; i++)
        {
            string str = _lines[i];
            _linePosBuffer[i - topIdx] = GetCharacterPosArray(graphics, ref str);
        }

        _linePosBufferTopLineIdx = topIdx;
    }
}



public struct ABC
{
    public int AbcA;
    public uint AbcB;
    public int AbcC;
} 