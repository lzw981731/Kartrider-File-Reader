using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Threading;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    // Refers to osu.Framework.Graphics.Menu
    public abstract partial class TabItemListBox: CompositeDrawable
    {
        protected readonly ScrollContainer<Drawable> ContentContainer;
        protected readonly Drawable Background;
        protected readonly Container MaskingContainer;

        private Colour4 backgroundColour;
        private FillFlowContainer<DrawableTabItem> itemsFlow;
        private DrawableTabItem? selectedTabItem;
        private Dictionary<TabItem, DrawableTabItem> tabItemDrawableTable = new Dictionary<TabItem, DrawableTabItem>();

        public event Action<TabItem> SelectedTabItemChanged;

        public TabItemList Items { get; } = new TabItemList();

        public Colour4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                Scheduler.AddOnce(UpdateBackgroundColour);
            }
        }

        public TabItem? SelectedItem => selectedTabItem?.Item;

        public IEnumerable<LocalisableString> FilterTerms => Items.Select(x => x.Text.Value);

        public Easing LayoutEasing
        {
            get => itemsFlow.LayoutEasing;
            set => itemsFlow.LayoutEasing = value;
        }

        public float LayoutDuration
        {
            get => itemsFlow.LayoutDuration;
            set => itemsFlow.LayoutDuration = value;
        }

        public new float CornerRadius
        {
            get => MaskingContainer.CornerRadius;
            set => MaskingContainer.CornerRadius = value;
        }

        protected Container<DrawableTabItem> ItemsContainer => itemsFlow;

        protected internal IReadOnlyList<DrawableTabItem> Children => itemsFlow.Children;

        protected TabItemListBox()
        {
            Items.OnInsert = onItemsInsert;
            Items.OnRemove = onItemsRemove;
            Items.OnClear = onItemsClear;

            InternalChild = MaskingContainer = new Container
            {
                Name = "Our contents",
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    Background = CreateBackground(),
                    ContentContainer = CreateScrollContainer(Direction.Horizontal).With(d =>
                    {
                        d.RelativeSizeAxes = Axes.Both;
                        d.Masking = true;
                        d.Child = itemsFlow = new ItemsFlow() { Direction = FillDirection.Horizontal };
                    }),
                }
            };

            itemsFlow.RelativeSizeAxes = Axes.Y;
            itemsFlow.AutoSizeAxes = Axes.X;
        }

        protected virtual Drawable CreateBackground() => new Box() { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableTabItem CreateDrawableTabItem(TabItem item);

        protected abstract ScrollContainer<Drawable> CreateScrollContainer(Direction direction);

        protected virtual void UpdateBackgroundColour()
        {
            Background.FadeColour(backgroundColour);
        }

        private void onItemsInsert(int index, TabItem item)
        {
            DrawableTabItem drawableItem = CreateDrawableTabItem(item);
            if (!tabItemDrawableTable.TryAdd(item, drawableItem))
                tabItemDrawableTable[item] = drawableItem;
            drawableItem.Clicked = selectItem;

            var items = Children.OrderBy(itemsFlow.GetLayoutPosition).ToList();

            for (int i = index; i < items.Count; i++)
                itemsFlow.SetLayoutPosition(items[i], i + 1);

            itemsFlow.Insert(index, drawableItem);
            if (itemsFlow.Count >= 1)
            {
                SchedulerAfterChildren.AddOnce(selectItem, drawableItem);
            }
            ((IItemsFlow)itemsFlow).SizeCache.Invalidate();
        }

        private void onItemsRemove(int index)
        {
            var items = Children.OrderBy(itemsFlow.GetLayoutPosition).ToList();
            for (int i = index + 1; i < items.Count; i++)
                itemsFlow.SetLayoutPosition(items[i], i - 1);
            var removeItem = items[index];
            if (removeItem.IsSelected && items.Count > 1)
            {
                if(index == 0)
                    selectItem(items[1]);
                else if((index + 1) < items.Count)
                    selectItem(items[index + 1]);
                else
                    selectItem(items[index - 1]);

            }

            tabItemDrawableTable.Remove(items[index].Item);
            itemsFlow.Remove(items[index], true);
            ((IItemsFlow)itemsFlow).SizeCache.Invalidate();
        }

        private void onItemsClear()
        {
            itemsFlow.Clear();
            tabItemDrawableTable.Clear();
        }

        private void selectItem(DrawableTabItem clickedItem)
        {
            if (clickedItem != selectedTabItem)
            {
                if (selectedTabItem is not null)
                    selectedTabItem.State = TabItemState.NotSelected;
                selectedTabItem = clickedItem;
                if (clickedItem is not null)
                {
                    selectedTabItem.State = TabItemState.Selected;
                    SelectedTabItemChanged?.Invoke(selectedTabItem.Item);
                    SchedulerAfterChildren.AddOnce(() => ContentContainer.ScrollIntoView(clickedItem));
                }
            }
        }

        public void SelectItem(TabItem tabItem)
        {
            if (tabItemDrawableTable.TryGetValue(tabItem, out DrawableTabItem? drawableItem))
            {
                selectItem(drawableItem);
            }
        }

        public abstract partial class DrawableTabItem: CompositeDrawable, IStateful<TabItemState>
        {
            public readonly TabItem Item;
            protected readonly Drawable Background;
            protected readonly Container Foreground;
            protected readonly Drawable Content;
            internal Action<DrawableTabItem> Clicked;

            private TabItemState state;
            private Colour4 backgroundColour;
            private Colour4 backgroundHoverColour;
            private Colour4 backgroundSelectedColour;
            private Colour4 foregroundColour = Colour4.White;
            private Colour4 foregroundHoverColour = Colour4.White;
            private Colour4 foregroundSelectedColour = Colour4.White;

            public event Action<TabItemState> StateChanged;

            public TabItemState State
            {
                get => state;
                set
                {
                    state = value;
                    Scheduler.AddOnce(() =>
                    {
                        UpdateBackgroundColour();
                        UpdateForegroundColour();
                    });
                    StateChanged?.Invoke(state);
                }
            }

            public bool IsSelected => state == TabItemState.Selected;

            public Colour4 BackgroundColour
            {
                get => backgroundColour;
                set
                {
                    backgroundColour = value;
                    Scheduler.AddOnce(UpdateBackgroundColour);
                }
            }

            public Colour4 BackgroundHoverColour
            {
                get => backgroundHoverColour;
                set
                {
                    backgroundHoverColour = value;
                    Scheduler.AddOnce(UpdateBackgroundColour);
                }
            }

            public Colour4 BackgroundSelectedColour
            {
                get => backgroundSelectedColour;
                set
                {
                    backgroundSelectedColour = value;
                    Scheduler.AddOnce(UpdateBackgroundColour);
                }
            }

            public Colour4 ForegroundColour
            {
                get => backgroundColour;
                set
                {
                    foregroundColour = value;
                    Scheduler.AddOnce(UpdateForegroundColour);
                }
            }

            public Colour4 ForegroundHoverColour
            {
                get => foregroundHoverColour;
                set
                {
                    foregroundHoverColour = value;
                    Scheduler.AddOnce(UpdateForegroundColour);
                }
            }

            public Colour4 ForegroundSelectedColour
            {
                get => foregroundSelectedColour;
                set
                {
                    foregroundSelectedColour = value;
                    Scheduler.AddOnce(UpdateForegroundColour);
                }
            }

            protected DrawableTabItem(TabItem item)
            {
                Item = item;
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;

                InternalChildren = new Drawable[]
                {
                    Background = CreateBackground(),
                    Foreground = new Container()
                    {
                        AutoSizeAxes = Axes.Both,
                        Child = Content = CreateContent()
                    }
                };

                if(Content is IHasText textContent)
                {
                    textContent.Text = Item.Text.Value;
                    Item.Text.ValueChanged += e => textContent.Text = e.NewValue;
                }

                if (Content is IStateful<TabItemState> stateful)
                {
                    stateful.State = this.State;
                    this.StateChanged += e => stateful.State = e;
                }
            }

            protected virtual void UpdateBackgroundColour()
            {
                Background.FadeColour(
                    IsSelected ? backgroundSelectedColour :
                    IsHovered ? backgroundHoverColour :
                    backgroundColour);
            }

            protected virtual void UpdateForegroundColour()
            {
                Foreground.FadeColour(
                    IsSelected ? foregroundSelectedColour :
                    IsHovered ? foregroundHoverColour :
                    foregroundColour);
            }

            protected virtual Drawable CreateBackground() => new Box() { RelativeSizeAxes = Axes.Both };

            protected abstract Drawable CreateContent();

            protected override bool OnHover(HoverEvent e)
            {
                Scheduler.AddOnce(UpdateBackgroundColour);
                Scheduler.AddOnce(UpdateForegroundColour);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Scheduler.AddOnce(UpdateBackgroundColour);
                Scheduler.AddOnce(UpdateForegroundColour);
            }

            protected override bool OnClick(ClickEvent e)
            {
                Clicked?.Invoke(this);
                Item.ClickAction.Value?.Invoke(this.Item);
                return true;
            }

            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                Item.DoubleClickAction.Value?.Invoke(this.Item);
                return true;
            }
        }

        internal interface IItemsFlow: IFillFlowContainer
        {
            LayoutValue SizeCache { get; }
        }

        internal partial class ItemsFlow: FillFlowContainer<DrawableTabItem>, IItemsFlow
        {
            public LayoutValue SizeCache => new LayoutValue(Invalidation.RequiredParentSizeToFit, InvalidationSource.Self);

            public ItemsFlow()
            {
                AddLayout(SizeCache);
            }
        }
    }

    public class TabItemList: IList<TabItem>
    {
        private readonly HashSet<TabItem> itemSearchingCache = new HashSet<TabItem>();
        private readonly List<TabItem> items = new List<TabItem>();

        internal Action<int, TabItem> OnInsert;
        internal Action<int> OnRemove;
        internal Action OnClear;

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public TabItem this[int index]
        {
            get => items[index];
            set
            {
                if(index >= Count)
                    throw new ArgumentOutOfRangeException("index");
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public void Add(TabItem item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            OnClear?.Invoke();
            items.Clear();
        }

        public bool Contains(TabItem item)
        {
            return itemSearchingCache.Contains(item);
        }

        public void CopyTo(TabItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TabItem item)
        {
            if(!Contains(item))
                return false;
            for(int i = 0; i < Count; i++)
                if (items[i] == item)
                {
                    RemoveAt(i);
                }
            return true;
        }

        public IEnumerator<TabItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(TabItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, TabItem item)
        {
            items.Insert(index, item);
            itemSearchingCache.Add(item);
            OnInsert?.Invoke(index, item);
        }

        public void RemoveAt(int index)
        {
            OnRemove?.Invoke(index);
            itemSearchingCache.Remove(items[index]);
            items.RemoveAt(index);
        }
    }

    public enum TabItemState
    {
        NotSelected,
        Selected,
    }
}
