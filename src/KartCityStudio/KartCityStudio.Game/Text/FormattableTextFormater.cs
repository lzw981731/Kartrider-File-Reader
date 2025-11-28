using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using eP.Text;

namespace KartCityStudio.Game.Text
{
    public class FormattableTextFormater: ITextFormater<string>
    {
        public int LevelDelta { get; set; } = 4;

        private List<TextFormat> textFormats = new List<TextFormat>();

        public void AddString(int level, TextAlign align, string text)
        {
            string[] lines = Regex.Split(text, "\\r\\n");
            foreach (string line in lines)
            {
                textFormats.Add(new TextFormat()
                {
                    Level = level,
                    Text = line,
                    Align = align
                });
            }
        }

        public string StartFormat()
        {
            List<string> topLine = new List<string>();
            List<string> bottomLine = new List<string>();
            foreach (TextFormat tf in textFormats)
            {
                switch (tf.Align)
                {
                    case TextAlign.Top:
                        topLine.Add($"{"".PadLeft(LevelDelta * tf.Level, ' ')}{tf.Text}");
                        break;
                    case TextAlign.Bottom:
                        bottomLine.Add($"{"".PadLeft(LevelDelta * tf.Level, ' ')}{tf.Text}");
                        break;
                }
            }
            List<string> output = new List<string>();
            foreach (string tl in topLine)
            {
                output.Add($@"{tl}");
            }
            bottomLine.Reverse();
            foreach (string tl in bottomLine)
            {
                output.Add($@"{tl}");
            }
            const string ftfHead = @"\$;\c@db:#008DDA;\c@b:#77CDFF;\c@or:#E38E49;";
            return  $"{ftfHead}\r\n{string.Join("\r\n", output)}\r\n";
        }
        public enum FormatTextCommand
        {
            EscapeText, EscapeArgument, NormalText, PaddingState
        }
    }
}
