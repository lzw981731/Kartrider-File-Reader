using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Localisation;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public class ListBoxItem
    {
        public readonly Bindable<LocalisableString> Text = new Bindable<LocalisableString>(string.Empty);

        public readonly Bindable<Action<ListBoxItem>?> ClickAction = new Bindable<Action<ListBoxItem>?>();

        public readonly Bindable<Action<ListBoxItem>?> DoubleClickAction = new Bindable<Action<ListBoxItem>?>();

        public object Tag;

        public ListBoxItem(LocalisableString text, Action<ListBoxItem>? clickAction = null, Action<ListBoxItem>? doubleClickAction = null, object tag = null)
        {
            Text.Value = text;
            ClickAction.Value = clickAction;
            DoubleClickAction.Value = doubleClickAction;
            Tag = tag;
        }
    }
}
