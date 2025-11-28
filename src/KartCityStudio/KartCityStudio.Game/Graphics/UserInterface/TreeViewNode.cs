using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public class TreeViewNode
    {
        public readonly Bindable<string> IconName = new Bindable<string>("");

        public readonly Bindable<LocalisableString> Text = new Bindable<LocalisableString>(string.Empty);

        public readonly TreeViewNodeList Nodes = new TreeViewNodeList();

        public readonly Bindable<bool> Expanded = new Bindable<bool>(false);

        public readonly Bindable<bool> HideExpandIcon = new Bindable<bool>(false);

        public readonly Bindable<bool> Selectable = new Bindable<bool>(true);

        public readonly Bindable<Action<TreeViewNode>?> ClickAction = new Bindable<Action<TreeViewNode>?>();

        public readonly Bindable<Action<TreeViewNode>?> DoubleClickAction = new Bindable<Action<TreeViewNode>?>();

        public object Tag;

        public TreeViewNode(LocalisableString text, string iconName = "", Action<TreeViewNode>? clickAction = null, Action<TreeViewNode>? doubleClickAction = null, object tag = null)
        {
            Text.Value = text;
            IconName.Value = iconName;
            ClickAction.Value = clickAction;
            DoubleClickAction.Value = doubleClickAction;
            Tag = tag;
        }
    }
}
