using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using KartCityStudio.Game.Graphics.Sprites;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    // Refers to osu.Framework.Graphics.Menu
    public abstract partial class TreeView: CompositeDrawable
    {
        protected ScrollContainer<Drawable> ContentContainer;
        protected Drawable Background;
        protected Container MaskingContainer;

        private Colour4 backgroundColour;
        private FillFlowContainer<DrawableTreeViewNodeFlow> nodeFlowsFlow;
        private DrawableTreeViewNode? selectedTreeViewNode;

        public TreeViewNodeList Nodes { get; } = new TreeViewNodeList();

        public Colour4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                Scheduler.AddOnce(UpdateBackgroundColour);
            }
        }

        public TreeViewNode? SelectedNode => selectedTreeViewNode?.Item;

        public IEnumerable<LocalisableString> FilterTerms => Nodes.Select(x => x.Text.Value);

        public new float CornerRadius
        {
            get => MaskingContainer.CornerRadius;
            set => MaskingContainer.CornerRadius = value;
        }

        protected Container<DrawableTreeViewNodeFlow> NodeFlowsContainer => nodeFlowsFlow;

        protected internal IReadOnlyList<DrawableTreeViewNodeFlow> Children => nodeFlowsFlow.Children;


        protected TreeView()
        {
            InternalChild = MaskingContainer = new Container
            {
                Name = "Our contents",
                RelativeSizeAxes = Axes.Both,
                Masking = false,
                Children = new Drawable[]
                {
                    Background = CreateBackground(),
                    ContentContainer = CreateScrollContainer(Direction.Vertical).With(d =>
                    {
                        d.RelativeSizeAxes = Axes.Both;
                        d.Masking = false;
                        d.Child = nodeFlowsFlow = new NodeFlowsFlow()
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        };
                    }),
                }
            };

            Nodes.OnInsert += onNodesInsert;
            Nodes.OnRemove += onNodesRemove;
            Nodes.OnClear += onNodesClear;
        }

        protected virtual Drawable CreateBackground() => new Box() { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableTreeViewNode CreateDrawableTreeViewItem(int depth, TreeViewNode item);

        protected abstract ScrollContainer<Drawable> CreateScrollContainer(Direction direction);

        protected virtual void UpdateBackgroundColour()
        {
            Background.FadeColour(backgroundColour);
        }

        private void onNodesInsert(int index, TreeViewNode item)
        {
            DrawableTreeViewNodeFlow drawableNodeFlow =
                new DrawableTreeViewNodeFlow(0, item, CreateDrawableTreeViewItem);
            drawableNodeFlow.SizeCacheInvalidated = sizeCacheInvalidated;
            drawableNodeFlow.NodeFlowClicked = onItemClicked;
            drawableNodeFlow.ExpandStateChanged = onExpandStateChanged;

            var nodeFlows = Children.OrderBy(nodeFlowsFlow.GetLayoutPosition).ToList();


            for (int i = index; i < nodeFlows.Count; i++)
                nodeFlowsFlow.SetLayoutPosition(nodeFlows[i], i + 1);

            nodeFlowsFlow.Insert(index, drawableNodeFlow);

            ((IItemsFlow)nodeFlowsFlow).SizeCache.Invalidate();
        }

        private void onNodesRemove(int index)
        {
            var nodeFlows = Children.OrderBy(nodeFlowsFlow.GetLayoutPosition).ToList();

            for (int i = index + 1; i < nodeFlows.Count; i++)
                nodeFlowsFlow.SetLayoutPosition(nodeFlows[i], i - 1);

            nodeFlowsFlow.Remove(nodeFlows[index], true);
            ((IItemsFlow)nodeFlowsFlow).SizeCache.Invalidate();
        }

        private void onNodesClear()
        {
            nodeFlowsFlow.Clear();
        }

        private void onExpandStateChanged(DrawableTreeViewNode obj)
        {
            if (obj.Item.Expanded?.Value == false && selectedTreeViewNode is not null)
            {
                DrawableTreeViewNode findNode = selectedTreeViewNode;
                while (findNode is not null && findNode != obj)
                {
                    findNode = findNode.ParentNode;
                }

                if (findNode is not null)
                {
                    onItemClicked(obj);
                }
            }
        }

        private void sizeCacheInvalidated()
        {
            ((IItemsFlow)nodeFlowsFlow).SizeCache.Invalidate();
        }

        private void onItemClicked(DrawableTreeViewNode clickedNode)
        {
            if (clickedNode.Item.Selectable.Value)
            {
                if (selectedTreeViewNode is not null)
                    selectedTreeViewNode.State = TreeViewItemState.NotSelected;

                selectedTreeViewNode = clickedNode;
                if(selectedTreeViewNode is not null)
                    selectedTreeViewNode.State = TreeViewItemState.Selected;
            }
        }

        protected internal delegate DrawableTreeViewNode CreateDrawableNodeDelegate(int depth, TreeViewNode item);

        // Item and its children
        protected internal partial class DrawableTreeViewNodeFlow : FillFlowContainer, IItemsFlow
        {
            private readonly int depth;

            private readonly CreateDrawableNodeDelegate createDrawableNodeFunc;

            private readonly NodeFlowsFlow nodeFlowsFlow;

            private DrawableTreeViewNodeFlow parentNodeFlow;

            private bool childInitialized;

            public Action SizeCacheInvalidated;

            public Action<DrawableTreeViewNode> NodeFlowClicked;

            public Action<DrawableTreeViewNode> ExpandStateChanged;

            internal DrawableTreeViewNode HeadTreeViewNode { get; init; }

            internal DrawableTreeViewNodeFlow ParentNodeFlow
            {
                get => parentNodeFlow;
                set
                {
                    parentNodeFlow = value;
                    if(HeadTreeViewNode is not null)
                        HeadTreeViewNode.ParentNode = parentNodeFlow?.HeadTreeViewNode;
                }
            }

            public TreeViewNode Item { get; init; }

            public LayoutValue SizeCache => new LayoutValue(Invalidation.RequiredParentSizeToFit, InvalidationSource.Self);

            public DrawableTreeViewNodeFlow(int depth, TreeViewNode item, CreateDrawableNodeDelegate createDrawableNodeFunc)
            {
                AddLayout(SizeCache);

                Item = item;

                Direction = FillDirection.Vertical;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                this.createDrawableNodeFunc = createDrawableNodeFunc;
                this.depth = depth;

                lock (item.Nodes)
                {
                    Children = new Drawable[]
                    {
                        HeadTreeViewNode = createDrawableNodeFunc(depth, item),
                        nodeFlowsFlow = new NodeFlowsFlow()
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = item.Expanded.Value ? 1f : 0f
                        }
                    };
                    if (item.Expanded.Value)
                    {
                        initializeChild();
                    }
                    else
                    {
                        childInitialized = false;
                    }
                    updatesExpandIcon();
                }

                HeadTreeViewNode.Clicked = onClicked;
                HeadTreeViewNode.ExpandStateChanged = expandStateChanged;
                HeadTreeViewNode.ParentNode = parentNodeFlow?.ParentNodeFlow.HeadTreeViewNode;

                item.Nodes.OnInsert += childNodesOnInsert;
                item.Nodes.OnRemove += childNodesOnRemove;
                item.Nodes.OnClear += childNodesOnClear;
            }

            private void initializeChild()
            {
                Logger.Log($"{nameof(initializeChild)}");
                if (!childInitialized)
                {
                    lock (Item.Nodes)
                    {
                        for (int i = 0; i < Item.Nodes.Count; i++)
                        {
                            TreeViewNode childNode = Item.Nodes[i];
                            DrawableTreeViewNodeFlow childNodeFlow =
                                new DrawableTreeViewNodeFlow(depth + 1, childNode, createDrawableNodeFunc);

                            // Binding some events to make a callback chain.
                            childNodeFlow.SizeCacheInvalidated = childSizeCacheInvalidated;
                            childNodeFlow.NodeFlowClicked = childOnClicked;
                            childNodeFlow.ExpandStateChanged = childExpandStateChanged;
                            childNodeFlow.ParentNodeFlow = this;

                            nodeFlowsFlow.Insert(i, childNodeFlow);
                        }

                        childInitialized = true;
                        Logger.Log($"{nameof(DrawableTreeViewNodeFlow)}: {this.GetHashCode():x8} waked up.");
                    }
                }
            }

            private void expandStateChanged(bool expanded, DrawableTreeViewNode obj)
            {
                ExpandStateChanged?.Invoke(this.HeadTreeViewNode);
                if (expanded)
                {
                    Scheduler.CancelDelayedTasks();
                    initializeChild();
                    nodeFlowsFlow.FadeIn();
                    fullInvalidation();
                }
                else
                {
                    Scheduler.AddDelayed(clearInitializedChilds, 300000);
                    nodeFlowsFlow.FadeOut();
                    partialInvalidation();
                }
            }

            private void onClicked(DrawableTreeViewNode obj)
            {
                NodeFlowClicked?.Invoke(obj);
            }

            private void fullInvalidation()
            {
                ((IItemsFlow)nodeFlowsFlow).SizeCache.Invalidate();
                if (Item.Expanded.Value)
                {
                    SizeCache.Invalidate();
                    SizeCacheInvalidated?.Invoke();
                }
            }

            private void partialInvalidation()
            {
                SizeCache.Invalidate();
                SizeCacheInvalidated?.Invoke();
            }

            private void updatesExpandIcon()
            {
                HeadTreeViewNode.ShowExpandIcon = Item.Nodes.Count > 0;
            }

            private void clearInitializedChilds()
            {
                lock (Item.Expanded)
                {
                    if (!Item.Expanded.Value && childInitialized)
                    {
                        nodeFlowsFlow.Clear();
                        childInitialized = false;
                        Logger.Log($"{nameof(DrawableTreeViewNodeFlow)}: {this.GetHashCode():x8} Lazied.");
                    }
                }
            }

            // Callback chain
            private void childExpandStateChanged(DrawableTreeViewNode obj)
            {
                ExpandStateChanged?.Invoke(obj);
            }

            private void childOnClicked(DrawableTreeViewNode obj)
            {
                NodeFlowClicked?.Invoke(obj);
            }

            private void childSizeCacheInvalidated()
            {
                fullInvalidation();
            }

            // Child modified handlers
            private void childNodesOnInsert(int index, TreeViewNode item)
            {
                // drawableNode.Clicked = onItemClicked;
                if (childInitialized)
                {
                    DrawableTreeViewNodeFlow childNodeFlow =
                        new DrawableTreeViewNodeFlow(depth + 1, item, createDrawableNodeFunc);

                    childNodeFlow.SizeCacheInvalidated = childSizeCacheInvalidated;
                    childNodeFlow.NodeFlowClicked = childOnClicked;
                    childNodeFlow.ExpandStateChanged = childExpandStateChanged;
                    childNodeFlow.ParentNodeFlow = this;

                    var nodes = nodeFlowsFlow.OrderBy(nodeFlowsFlow.GetLayoutPosition).ToList();

                    for (int i = index; i < nodes.Count; i++)
                        nodeFlowsFlow.SetLayoutPosition(nodes[i], i + 1);

                    nodeFlowsFlow.Insert(index, childNodeFlow);

                    fullInvalidation();
                }

                updatesExpandIcon();
            }

            private void childNodesOnRemove(int index)
            {
                if (childInitialized)
                {
                    var nodes = nodeFlowsFlow.OrderBy(nodeFlowsFlow.GetLayoutPosition).ToList();

                    for (int i = index + 1; i < nodes.Count; i++)
                        nodeFlowsFlow.SetLayoutPosition(nodes[i], i - 1);

                    DrawableTreeViewNodeFlow childNodeFlow = nodes[index];
                    childNodeFlow.ParentNodeFlow = null;
                    childNodeFlow.SizeCacheInvalidated = null;
                    childNodeFlow.NodeFlowClicked = null;
                    childNodeFlow.ExpandStateChanged = null;
                    childNodeFlow.ParentNodeFlow = this;
                    nodeFlowsFlow.Remove(childNodeFlow, true);

                    fullInvalidation();
                }

                updatesExpandIcon();
            }

            private void childNodesOnClear()
            {
                nodeFlowsFlow.Clear();

                if(childInitialized)
                    partialInvalidation();

                updatesExpandIcon();
            }
        }

        // Item only
        public abstract partial class DrawableTreeViewNode: CompositeDrawable, IStateful<TreeViewItemState>
        {
            public readonly TreeViewNode Item;
            protected Drawable Background;
            protected FillFlowContainer Foreground;
            protected SpriteIcon ExpandStateIcon;
            protected ClickableContainer ExpandContainer;
            protected Drawable Content;

            internal Action<DrawableTreeViewNode> Clicked;
            internal Action<bool, DrawableTreeViewNode> ExpandStateChanged;

            private TreeViewItemState state;
            private Colour4 backgroundColour;
            private Colour4 backgroundHoverColour;
            private Colour4 backgroundSelectedColour;
            private Colour4 foregroundColour = Colour4.White;
            private Colour4 foregroundHoverColour = Colour4.White;
            private Colour4 foregroundSelectedColour = Colour4.White;
            private bool showExpandIcon;
            private bool alwaysHideExpandIcon;
            private int depth;

            public event Action<TreeViewItemState> StateChanged;

            public TreeViewItemState State
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
                }
            }

            public bool IsSelected => state == TreeViewItemState.Selected;

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

            public bool ShowExpandIcon
            {
                get => showExpandIcon;
                set
                {
                    showExpandIcon = value;
                    Scheduler.AddOnce(updateExpandIconVisibility);
                }
            }

            public DrawableTreeViewNode ParentNode { get; set; }

            public bool AlwaysHideExpandIcon
            {
                get => alwaysHideExpandIcon;
                set
                {
                    alwaysHideExpandIcon = value;
                    Scheduler.AddOnce(updateExpandIconVisibility);
                }
            }

            protected DrawableTreeViewNode(int depth, TreeViewNode item)
            {
                this.Item = item;
                this.depth = depth;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Item.Expanded.ValueChanged += expandedValueChanged;
                alwaysHideExpandIcon = Item.HideExpandIcon.Value;
                Item.HideExpandIcon.ValueChanged += e => AlwaysHideExpandIcon = e.NewValue;

                InternalChildren = new Drawable[]
                {
                    Background = CreateBackground(),
                    Foreground = new FillFlowContainer()
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding()
                        {
                            Horizontal = 10f + 20f * depth,
                        },
                        Children = new Drawable[]
                        {
                            ExpandContainer = new ClickableContainer()
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.None,
                                Width = 20,
                                Height = 9,
                                Child = ExpandStateIcon = new SpriteIcon()
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.None,
                                    RelativePositionAxes = Axes.None,
                                    Width = 9,
                                    Height = 9,
                                    Position = new Vector2(4.5f, 0),
                                    Rotation = Item.Expanded.Value ? 90 : 0,
                                    Icon = FontAwesome.Solid.ChevronRight,
                                    Colour = Colour4.White,
                                },
                            },
                            Content = CreateContent().With(d =>
                            {

                            }),
                        }
                    },
                };
                ExpandContainer.Action = expandContainerClicked;
                if(Content is IHasText textContent)
                {
                    textContent.Text = Item.Text.Value;
                    Item.Text.ValueChanged += e => textContent.Text = e.NewValue;
                }

                if (Content is IHasIcon iconContent)
                {
                    iconContent.IconTextureName = Item.IconName.Value;
                    Item.IconName.ValueChanged += e => iconContent.IconTextureName = e.NewValue;
                }
            }

            private void expandContainerClicked()
            {
                if (ExpandStateIcon.Alpha < 0.3f)
                {
                    Clicked?.Invoke(this);
                }
                else
                {
                    Item.Expanded.Value ^= true;
                }
            }

            private void expandedValueChanged(ValueChangedEvent<bool> obj)
            {
                if (obj.NewValue)
                    ExpandStateIcon.RotateTo(90, 320, Easing.OutQuint);
                else
                    ExpandStateIcon.RotateTo(0, 320, Easing.OutQuint);
                ExpandStateChanged?.Invoke(obj.NewValue, this);
            }

            protected virtual void UpdateBackgroundColour()
            {
                Background.FadeColour(
                    IsSelected ? backgroundSelectedColour :
                    (IsHovered && Item.Selectable.Value) ? backgroundHoverColour :
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

            private void updateExpandIconVisibility()
            {
                if (showExpandIcon && (!alwaysHideExpandIcon))
                {
                    ExpandContainer.FadeIn();
                    ExpandStateIcon.FadeIn();
                }
                else
                {
                    if (alwaysHideExpandIcon)
                        ExpandContainer.FadeOut();
                    else
                        ExpandStateIcon.FadeOut();
                }
            }
        }

        internal interface IItemsFlow: IFillFlowContainer
        {
            LayoutValue SizeCache { get; }
        }

        internal partial class NodeFlowsFlow: FillFlowContainer<DrawableTreeViewNodeFlow>, IItemsFlow
        {
            public LayoutValue SizeCache => new LayoutValue(Invalidation.RequiredParentSizeToFit, InvalidationSource.Self);

            public NodeFlowsFlow()
            {
                AddLayout(SizeCache);
            }
        }
    }

    public delegate void NodeInsertDelegate(int index, TreeViewNode item);

    public delegate void NodeRemoveDelegate(int index);

    public delegate void NodeClearDelegate();

    public class TreeViewNodeList: IList<TreeViewNode>
    {
        private readonly HashSet<TreeViewNode> itemSearchingCache = new HashSet<TreeViewNode>();
        private readonly List<TreeViewNode> items = new List<TreeViewNode>();

        internal event NodeInsertDelegate OnInsert;
        internal event NodeRemoveDelegate OnRemove;
        internal event NodeClearDelegate OnClear;

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public TreeViewNode this[int index]
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

        public void Add(TreeViewNode item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            OnClear?.Invoke();
            items.Clear();
        }

        public bool Contains(TreeViewNode item)
        {
            return itemSearchingCache.Contains(item);
        }

        public void CopyTo(TreeViewNode[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TreeViewNode item)
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

        public IEnumerator<TreeViewNode> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(TreeViewNode item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, TreeViewNode item)
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

    public enum TreeViewItemState
    {
        NotSelected,
        Selected,
    }
}
