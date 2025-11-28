using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using eP.Text;
using KartCity.Common.Xml;
using KartCityStudio.Game.Extension;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Text;
using KartLibrary.Xml;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSXmlView : TreeView
    {
        private readonly FormattableTextParser viewParser = new FormattableTextParser();
        public KCSXmlView()
        {
            viewParser.Parse(@"\$;\c@db:#008DDA;\c@b:#77CDFF;\c@or:#E38E49;");
            NodeFlowsContainer.Padding = new MarginPadding { Horizontal = 10, Vertical = 0 };
        }

        protected override DrawableTreeViewNode CreateDrawableTreeViewItem(int depth, TreeViewNode item) => new KCSXmlViewNode(0, item)
        {
            Parser = viewParser
        };

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new KCSScrollContainer<Drawable>()
        {
            AutoHideScrollerBar = true
        };

        public void LoadXml(BinaryXmlTag binaryXmlTag)
        {
            TreeViewNode[] nodes = binaryXmlTag.ToCustomFormaterText(true, 0, new XmlNodeFormater());
            nodes.ForEach(x => Nodes.Add(x));
        }

        private TreeViewNode loadXmlToNode(BinaryXmlTag binaryXmlTag, int level)
        {
            string formattedText = binaryXmlTag.ToFormattableText(false, level);
            string[] splitFormattedText = formattedText.Split('\n');
            TreeViewNode output = new TreeViewNode(
                splitFormattedText[1]
            );
            foreach (var child in binaryXmlTag.Children)
                output.Nodes.Add(loadXmlToNode(child, level + 1));
            if(splitFormattedText.Length >= 2)
                output.Nodes.Add(new TreeViewNode(
                    splitFormattedText[2]
                ));
            return output;
        }

        private class XmlNodeFormater : ITextFormater<TreeViewNode[]>
        {
            public int LevelDelta => 4;

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

            public TreeViewNode[] StartFormat()
            {
                List<TextFormat> topFormats = new();
                List<TextFormat> bottomFormats = new();
                foreach (TextFormat tf in textFormats)
                {
                    switch (tf.Align)
                    {
                        case TextAlign.Top:
                            topFormats.Add(tf);
                            break;
                        case TextAlign.Bottom:
                            bottomFormats.Add(tf);
                            break;
                    }
                }

                List<TextFormat> combinedFormats = [..topFormats, ..bottomFormats];
                List<TreeViewNode> rootNodes = new List<TreeViewNode>();
                Stack<TreeViewNode> stack = new Stack<TreeViewNode>();
                foreach (var textFormat in combinedFormats)
                {
                    TreeViewNode newNode = new TreeViewNode(
                        text: textFormat.Text.PadLeft(textFormat.Text.Length + textFormat.Level * LevelDelta, ' '),
                        ""
                    );
                    int stackLevel = stack.Count - 1;
                    do
                    {
                        stackLevel = stack.Count - 1;
                        if (stackLevel < textFormat.Level)
                        {
                            if(stackLevel >= 0)
                                stack.Peek().Nodes.Add(newNode);
                            else
                                rootNodes.Add(newNode);
                            stack.Push(newNode);
                        }
                        else
                        {
                            stack.Pop();
                        }
                    } while (stackLevel >= textFormat.Level);
                }

                return rootNodes.ToArray();
            }
        }
    }
}
