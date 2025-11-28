using System;
using System.Collections.Generic;
using KartCityStudio.Game.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;

namespace KartCityStudio.Game.Graphics.Containers;

public abstract partial class TabControlContainer: Container<TabPageContainer>
{
    protected TabItemListBox TabItemListBox { get; init; }

    private Container<TabPageContainer> contentContainer;

    private HashSet<TabPageContainer> searchSet = new HashSet<TabPageContainer>();

    protected override Container<TabPageContainer> Content => contentContainer;

    protected abstract TabItemListBox CreateTabItemListBox();

    public new int Count => searchSet.Count;

    protected TabControlContainer()
    {
        TabItemListBox = CreateTabItemListBox();
        TabItemListBox.Anchor = Anchor.TopLeft;
        TabItemListBox.Origin = Anchor.TopLeft;
        TabItemListBox.SelectedTabItemChanged += selectedTabChanged;
        InternalChildren = new Drawable[]
        {
            contentContainer = new Container<TabPageContainer>()
            {
                RelativeSizeAxes = Axes.Both,
            },
            TabItemListBox,
        };
        if ((int)(TabItemListBox.RelativeSizeAxes & Axes.Y) != 0)
        {
            contentContainer.RelativePositionAxes |= Axes.Y;
            contentContainer.Y = TabItemListBox.Height;
            contentContainer.Height = 1 - TabItemListBox.Height;
        }
        else
        {
            contentContainer.Padding = new MarginPadding() { Top = TabItemListBox.Height };
        }
    }

    private void selectedTabChanged(TabItem selectedObj)
    {
        Logger.Log($"{searchSet.Count}");
        if (selectedObj.Tag is TabPageContainer pageContainer && searchSet.Contains(pageContainer))
        {
            Content.Clear(false);
            Content.Child = pageContainer;
            Logger.Log($"Setted");
        }
    }

    public override void Add(TabPageContainer drawable)
    {
        if (drawable is not null && !searchSet.Contains(drawable))
        {
            TabItem tabItem = new TabItem(drawable.Text);
            tabItem.Tag = drawable;
            tabItem.Pinned.Value = drawable.Pinned.Value;
            if (!drawable.Pinned.Value)
                tabItem.CloseAction.Value = e =>
                {
                    if (e.Tag is TabPageContainer pageContainer && searchSet.Contains(pageContainer))
                        Remove(pageContainer, true);
                };
            drawable.Pinned.ValueChanged += @event => tabItem.Pinned.Value = @event.NewValue;
            drawable.TargetTabItem = tabItem;
            drawable.TextChanged += e => tabItem.Text.Value = e;
            searchSet.Add(drawable);
            TabItemListBox.Items.Add(tabItem);
        }
    }

    public void SwitchToTab(TabPageContainer drawable)
    {
        if (searchSet.Contains(drawable))
        {
            TabItemListBox.SelectItem(drawable.TargetTabItem);
        }
    }

    public override bool Remove(TabPageContainer drawable, bool disposeImmediately)
    {
        if (searchSet.Contains(drawable))
        {
            if (contentContainer.Count > 0 && contentContainer.Child == drawable)
                contentContainer.Children = Array.Empty<TabPageContainer>();
            if (drawable.TargetTabItem != null)
                TabItemListBox.Items.Remove(drawable.TargetTabItem);
            searchSet.Remove(drawable);
            if(disposeImmediately)
                drawable.RemoveAndDisposeImmediately();
            return true;
        }

        return false;
    }
}

public partial class TabPageContainer : Container, IHasText
{
    private LocalisableString text;

    internal TabItem TargetTabItem;

    public event Action<LocalisableString> TextChanged;

    public readonly Bindable<bool> Pinned = new Bindable<bool>(false);

    public LocalisableString Text
    {
        get => text;
        set
        {
            text = value;
            TextChanged?.Invoke(value);
        }
    }


    public TabPageContainer(LocalisableString text)
    {
        this.text = text;
    }
}
