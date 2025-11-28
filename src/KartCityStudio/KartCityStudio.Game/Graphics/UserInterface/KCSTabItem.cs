using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSTabItem: TabItemListBox.DrawableTabItem
    {
        #region Members
        private KCSTabItemContentContainer content;
        private KCSTabItemContentContainer menuItemContentContainer;
        private Container backgroundContainer;
        private Box backgroundBox;
        private Box selectedBar;
        private Colour4 selectedBarColour;
        #endregion

        #region Properies

        #endregion
        #region Constructors
        public KCSTabItem(TabItem item) : base(item)
        {

        }
        #endregion

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Colour4.Transparent;
            BackgroundHoverColour = Colour4.FromHex("1A1A1A");
            BackgroundSelectedColour = Colour4.FromHex("1C1C1F");
            selectedBarColour = Colour4.FromHex("3A3A3A");
            BorderColour = Colour4.Transparent;
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Foreground.Anchor = Anchor.CentreLeft;
            Foreground.Origin = Anchor.CentreLeft;
        }

        protected sealed override Drawable CreateContent()
        {
            menuItemContentContainer = new KCSTabItemContentContainer()
            {
            };
            menuItemContentContainer.Pinned.Value = Item.Pinned.Value;
            Item.Pinned.ValueChanged += @event => menuItemContentContainer.Pinned.Value = @event.NewValue;
            menuItemContentContainer.CloseButtonClicked += closeButtonClicked;
            return menuItemContentContainer;
        }

        protected override Drawable CreateBackground() => backgroundContainer = new Container()
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                backgroundBox = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour,
                    Alpha = 0
                },
                new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Padding = new MarginPadding(){ Horizontal = 0f },
                    Size = new Vector2(1, 1),
                    Child = selectedBar = new Box()
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Size = new Vector2(1, 2f),
                        Colour = Colour4.Transparent,
                    }
                },
            }
        };

        protected override void UpdateBackgroundColour()
        {
            backgroundBox.FadeColour(
                IsSelected ? BackgroundSelectedColour :
                IsHovered ? BackgroundHoverColour :
                BackgroundColour);
            selectedBar.FadeColour(
                IsSelected ? selectedBarColour :
                    Colour4.Transparent);
        }

        private void closeButtonClicked()
        {
            Item.CloseAction.Value?.Invoke(Item);
        }

        protected partial class KCSTabItemContentContainer : Container, IHasText, IStateful<TabItemState>
        {
            private readonly SpriteIcon listBoxItemIcon;
            private readonly SpriteText listBoxItemText;
            private readonly KCSIconButton closeTabBtn;
            private LocalisableString text;
            private TabItemState state;

            public TabItemState State
            {
                get => state;
                set => onStateChanged(value);
            }

            public event Action<TabItemState> StateChanged;

            public event Action CloseButtonClicked;

            public Bindable<bool> Pinned { get; } = new Bindable<bool>(false);

            public LocalisableString Text
            {
                get => text;
                set
                {
                    text = value;
                    listBoxItemText.Text = value;
                }
            }

            public KCSTabItemContentContainer()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    listBoxItemText = new SpriteText()
                    {
                        AlwaysPresent = true,
                        Font = KCSFont.Default,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Shadow = true,
                        Margin = new MarginPadding { Left = 15, Right = 25, Vertical = 4 }
                    },
                    closeTabBtn = new KCSIconButton()
                    {
                        Action = closedButtonClicked,
                        Icon = FontAwesome.Solid.Times,
                        Size = new Vector2(17f, 17f),
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        BackgroundColour = Colour4.Transparent,
                        Position = new Vector2(-3f, 0f),
                        CornerRadius = 7.5f,
                        ScaleWhenButtonDown = false,
                        HoverColour = Colour4.FromHex("2A2A2A"),
                        IconRelativeSize = new Vector2(0.5f),
                        Alpha = 0f,
                    }
                };

                this.StateChanged += onStateChanged;
                this.Pinned.ValueChanged += @event =>
                {
                    if (@event.NewValue)
                    {
                        closeTabBtn.FadeOut();
                    }
                    else if (State == TabItemState.Selected)
                    {
                        closeTabBtn.FadeIn();
                    }
                };
            }

            private void closedButtonClicked()
            {
                CloseButtonClicked?.Invoke();
            }

            private void onStateChanged(TabItemState obj)
            {
                state = obj;
                if (obj == TabItemState.Selected && !Pinned.Value)
                {
                    closeTabBtn.FadeIn();
                }
                else
                {
                    closeTabBtn.FadeOut();
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                if(!Pinned.Value)
                    closeTabBtn.FadeIn();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if(state == TabItemState.NotSelected)
                    closeTabBtn.FadeOut();
                base.OnHoverLost(e);
            }
        }
    }
}
