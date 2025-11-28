using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;
using osu.Framework.Graphics;

namespace KartCityStudio.Game.Text;

public class FormattableTextParser
{
    private readonly HashSet<char> normalTerminalCharSet = new HashSet<char>()
    {
        '\\'
    };

    private readonly HashSet<char> escapeCharSet = new HashSet<char>()
    {
        '\\', ';', ':'
    };

    private readonly HashSet<char> escapeArgsCharSet = new HashSet<char>()
    {
        ';', '\\', ':', ' '
    };


    private Dictionary<string, MacroInfo> macros = new Dictionary<string, MacroInfo>();

    public FormattableTextParser()
    {

    }

    public StylizedText[][] Parse(string text)
    {
        List<List<StylizedText>> output = new List<List<StylizedText>>();
        StringBuilder tmpStringBuilder = new StringBuilder();
        StylizedText tmpStylizedText = new StylizedText();
        Queue<string> tmpQueue = new Queue<string>();
        ParseState parseState = ParseState.Normal;

        TextLineReader reader = new TextLineReader(text);
        do
        {
            List<StylizedText> lineResults = new List<StylizedText>();
            bool ignoreLine = false;
            while (!reader.IsEndOfLine)
            {
                if (parseState == ParseState.Normal)
                {
                    string str = reader.ReadUntil(normalTerminalCharSet);
                    tmpStringBuilder.Append(str);
                    if (reader.AcceptChar('\\'))
                    {
                        if (reader.AcceptChar('\\'))
                        {
                            tmpStringBuilder.Append('\\');
                            continue;
                        }
                        else
                        {
                            parseState = ParseState.EscapeMode;
                        }
                    }

                    if (tmpStringBuilder.Length > 0)
                    {
                        if(!ignoreLine)
                            lineResults.Add(tmpStylizedText with { Text = tmpStringBuilder.ToString() });
                    }

                    tmpStringBuilder.Clear();
                }
                else if (parseState == ParseState.EscapeMode)
                {
                    string commandName = reader.ReadUntil(escapeCharSet);
                    if (commandName.Length == 0 && reader.PeekChar() == '\\')
                    {

                    }
                    else
                    {
                        if (commandName.Length > 0)
                            tmpQueue.Enqueue(commandName);
                        else
                            throw new Exception("command name required");
                        if (reader.AcceptChar(':'))
                            parseState = ParseState.EscapeModeArgument;
                        else
                        {
                            parseState = ParseState.EscapeModeProcess;
                        }
                    }
                }
                else if (parseState == ParseState.EscapeModeArgument)
                {
                    string commandArg = reader.ReadUntil(escapeArgsCharSet);
                    tmpQueue.Enqueue(commandArg);

                    parseState = ParseState.EscapeModeProcess;
                }
                else if (parseState == ParseState.EscapeModeProcess)
                {
                    string cmdName = tmpQueue.Dequeue();
                    string? arg = tmpQueue.Count != 0 ? tmpQueue.Dequeue() : null;
                    if (cmdName == "$")
                    {
                        ignoreLine = true;
                    }

                    else
                        processingCmd(cmdName, arg,ref tmpStylizedText);

                    tmpQueue.Clear();

                    char curCh = reader.PeekChar();
                    if(reader.AcceptChar('\\'))
                        parseState = ParseState.EscapeMode;
                    else
                    {
                        parseState = ParseState.Normal;
                        reader.ReadChar();
                    }

                }
                else
                {
                    throw new Exception("unknown state.");
                }
            }
            if(!ignoreLine)
                output.Add(lineResults);
        } while (reader.MoveNextLine());

        return output.Select(x => x.ToArray()).ToArray();
    }

    private void processingCmd(string cmdName, string? arg, ref  StylizedText stylizedText)
    {
        string? asMacro = null;
        int atSymbolPos = cmdName.IndexOf('@');
        if (atSymbolPos >= 0)
        {
            string newCmdName = cmdName[..atSymbolPos];
            asMacro = cmdName[atSymbolPos..];
            cmdName = newCmdName;
        }

        if (cmdName.Length == 0 && asMacro is not null)
        {
            if (!macros.TryGetValue(asMacro, out var macroInfo))
                throw new Exception();
            cmdName = macroInfo.TargetCommandName;
            arg = macroInfo.TargetCommandArgument;
        }

        if (cmdName == "c")
        {
            if (arg is null) throw new Exception("argument required.");
            stylizedText.Colour = Colour4.FromHex(arg);
        }
        else if (cmdName == "s")
        {
            if (arg is null) throw new Exception("argument required.");
            stylizedText.Size = float.Parse(arg);
        }
        else if (cmdName == "b") stylizedText.Bold = true;
        else if (cmdName == "i") stylizedText.Italic = true;
        else if (cmdName == "u")
        {
            Dictionary<string, bool> unsetFields = new Dictionary<string, bool>()
            {
                ["c"] = false, ["s"] = false, ["b"] = false, ["i"] = false
            };

            if (arg is null)
                foreach (string key in unsetFields.Keys)
                    unsetFields[key] = true;
            else
                foreach(char unsetField in arg)
                    if (unsetFields.ContainsKey($"{unsetField}"))
                        unsetFields[$"{unsetField}"] = true;

            if (unsetFields["c"])
                stylizedText.Colour = null;
            if (unsetFields["s"])
                stylizedText.Size = null;
            if (unsetFields["b"])
                stylizedText.Bold = null;
            if (unsetFields["i"])
                stylizedText.Italic = null;
        }
        else
        {
            throw new Exception("unknown command.");
        }

        if (asMacro is not null)
        {
            if (!macros.TryGetValue(asMacro, out var macroInfo))
            {
                macroInfo = new MacroInfo();
                macros.Add(asMacro, macroInfo);
            }

            macroInfo.TargetCommandName = cmdName;
            macroInfo.TargetCommandArgument = arg;
        }
    }

    private enum ParseState
    {
        Normal,
        EscapeMode,
        EscapeModeArgument,
        EscapeModeProcess,
    }
}

public class MacroInfo
{
    public string MacroName { get; set; } = "";
    public string TargetCommandName { get; set; } = "";
    public string? TargetCommandArgument { get; set; }
}

public struct StylizedText
{
    public string Text { get; set; }

    public bool? Bold { get; set; }

    public bool? Italic { get; set; }

    public float? Size { get; set; }

    public Colour4? Colour { get; set; }
}
