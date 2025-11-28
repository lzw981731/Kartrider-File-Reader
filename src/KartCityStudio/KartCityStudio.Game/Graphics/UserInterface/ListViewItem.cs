using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Localisation;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public class ListViewItem
    {
        public readonly BindableDictionary<string, LocalisableString> Texts = new BindableDictionary<string, LocalisableString>();

        public readonly Bindable<string> IconName = new Bindable<string>("");

        public readonly Bindable<object?> Tag = new Bindable<object?>();

        public readonly Bindable<bool> Visible = new Bindable<bool>(true);

        internal readonly Bindable<Action<ListViewItem>?> ClickAction = new Bindable<Action<ListViewItem>?>();

        internal readonly Bindable<Action<ListViewItem>?> DoubleClickAction = new Bindable<Action<ListViewItem>?>();

        public ListViewItem((string key, LocalisableString value)[] texts, string iconName = "")
        {
            IconName.Value = iconName;
            texts.Select(x => new KeyValuePair<string, LocalisableString>(x.key, x.value)).ForEach(x => Texts.Add(x.Key, x.Value));
        }

        public ListViewItem(KeyValuePair<string, LocalisableString>[] texts, string iconName = "")
        {
            IconName.Value = iconName;
            texts.ForEach(x => Texts.Add(x.Key, x.Value));
        }
    }
}
