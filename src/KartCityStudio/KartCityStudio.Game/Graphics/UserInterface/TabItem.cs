using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public class TabItem
    {
        public readonly Bindable<LocalisableString> Text = new Bindable<LocalisableString>(string.Empty);

        public readonly Bindable<Action<TabItem>?> ClickAction = new Bindable<Action<TabItem>?>();

        public readonly Bindable<Action<TabItem>?> DoubleClickAction = new Bindable<Action<TabItem>?>();

        public readonly Bindable<Action<TabItem>?> CloseAction = new Bindable<Action<TabItem>?>();

        public readonly Bindable<bool> Pinned = new Bindable<bool>();

        public object Tag;

        public TabItem(LocalisableString text, Action<TabItem>? clickAction = null, Action<TabItem>? doubleClickAction = null, Action<TabItem> closeAction = null,  object tag = null, bool pinned = false)
        {
            Text.Value = text;
            ClickAction.Value = clickAction;
            DoubleClickAction.Value = doubleClickAction;
            CloseAction.Value = closeAction;
            Pinned.Value = pinned;
            Tag = tag;
        }
    }
}
