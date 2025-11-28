using System.Collections.Generic;
using KartCityStudio.Game.Text;
using eP.Text;
using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartCityStudio.Game.Extension;

public static class FormattableTextExt
{
    public static string ToFormattableText(this BinaryXmlTag bxt)
    {
        return bxt.ToFormattableText(recursion: true);
    }

    public static string ToFormattableText(this BinaryXmlTag bxt, bool recursion)
    {
        return bxt.ToFormattableText(recursion, 0);
    }

    public static string ToFormattableText(this BinaryXmlTag bxt, bool recursion, int level)
    {
        FormattableTextFormater formater = new FormattableTextFormater();
        bxt.applyToFormattableText(formater, level, recursion);
        return formater.StartFormat();
    }

    public static T ToCustomFormaterText<T>(this BinaryXmlTag bxt, bool recursion, int level, ITextFormater<T> formater)
    {
        bxt.applyToFormattableText(formater, level, recursion);
        return formater.StartFormat();
    }

    private static void applyToFormattableText<T>(this BinaryXmlTag bxt, ITextFormater<T> formater, int nowLevel, bool recursion)
    {
        bool haveText = bxt.Text != null && bxt.Text != "";
        bool haveAttributes = bxt.Attributes.Count > 0;
        bool haveSubTag = bxt.Children.Count > 0;
        string start = "";
        string att = "";
        string end = "";
        string addition = "";
        bool oneLine = true;
        if ((haveText || haveSubTag))
        {
            end = $"\\@db;</{bxt.Name}>";
            oneLine = !haveSubTag;
        }
        else
        {
            end = $"";
            oneLine = true;
            addition = "/";
        }
        if (haveAttributes)
        {
            List<string> attFormat = new List<string>();
            foreach (KeyValuePair<string, string> KeyPair in bxt.Attributes)
            {
                attFormat.Add($"\\@b;{KeyPair.Key}\\@db;=\"\\@or;{KeyPair.Value.Replace("\\", "\\\\").Replace("\"", "&quot;")}\\@db;\"");
            }
            att = $" {string.Join(" ", attFormat)}";
        }
        start = $"\\@db;<{bxt.Name}{att}{addition}>";
        if (oneLine)
        {
            formater.AddString(nowLevel, TextAlign.Top, $"{start}{bxt.Text.Replace("\\", "\\\\").Replace("\"", "&quot;") ?? ""}{end}");
        }
        else
        {
            formater.AddString(nowLevel, TextAlign.Top, $"{start}{bxt.Text.Replace("\\", "\\\\").Replace("\"", "&quot;") ?? ""}");
            if (recursion)
            {
                foreach (BinaryXmlTag sub in bxt.Children)
                {
                    sub.applyToFormattableText(formater, nowLevel + 1, recursion);
                }
            }
            formater.AddString(nowLevel, TextAlign.Top, end);
        }
    }
}
