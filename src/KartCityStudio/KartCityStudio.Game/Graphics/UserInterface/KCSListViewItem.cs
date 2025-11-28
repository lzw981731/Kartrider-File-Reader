using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSListViewItem: ListView.DrawableListViewItem
    {
        #region Members
        private KCSSubMenuItemTextContainer text;
        #endregion
        #region Properies
        #endregion
        #region Constructors
        public KCSListViewItem(ListViewItem item) : base(item)
        {
            Margin = new MarginPadding { Vertical = 2 };
        }
        #endregion

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore)
        {
            BackgroundColour = Colour4.Transparent;
            BackgroundHoverColour = Colour4.FromHex("1A1A1A");
            BackgroundSelectedColour = Colour4.FromHex("3887BE");
            BorderColour = Colour4.Transparent;
            Masking = true;
            CornerRadius = 5f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Foreground.Anchor = Anchor.CentreLeft;
            Foreground.Origin = Anchor.CentreLeft;
        }

        protected sealed override Drawable CreateContent() => text = CreateTextContainer();

        protected virtual KCSSubMenuItemTextContainer CreateTextContainer() => new KCSSubMenuItemTextContainer();

        protected override void MakeVisible()
        {
            this.FadeIn(320, Easing.OutQuint);
        }

        protected override void MakeInvisible()
        {
            this.FadeOut();
        }

        protected partial class KCSSubMenuItemTextContainer : Container, IHasText, IHasIcon
        {
            private readonly Container listViewItemIconContainer;
            private readonly Sprite listViewItemIcon;
            private readonly SpriteText listBoxItemText;
            private LocalisableString text;
            private TextureStore textureStore;

            private string iconTextureName = "";

            public LocalisableString Text
            {
                get => text;
                set
                {
                    text = value;
                    listBoxItemText.Text = value;
                }
            }

            public string IconTextureName
            {
                get => iconTextureName;
                set
                {
                    iconTextureName = value;
                    loadIcon();
                }
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textureStore)
            {
                this.textureStore = textureStore;
                loadIcon();
            }

            public KCSSubMenuItemTextContainer()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Y;
                Child = new FillFlowContainer()
                {
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.X,
                    Height = 25f,
                    Margin = new MarginPadding() { Left = 22f },
                    Children = new Drawable[]
                    {
                        listViewItemIconContainer = new Container()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.None,
                            Width = 16f,
                            Height = 16f,
                            Alpha = 0f,
                            Child = listViewItemIcon = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.None,
                                TextureRelativeSizeAxes = Axes.Both
                            },
                            Margin = new MarginPadding() { Right = 10 }
                        },
                        listBoxItemText = new SpriteText()
                        {
                            AlwaysPresent = true,
                            Font = KCSFont.Default,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Shadow = true,
                            Margin = new MarginPadding { Vertical = 4 }
                        }
                    }
                };
            }

            private void loadIcon()
            {
                if (textureStore is not null && iconTextureName.Length > 0 && listViewItemIcon is not null)
                {
                    listViewItemIcon.Texture = textureStore.Get(iconTextureName);
                    if (listViewItemIcon.Texture is not null)
                    {
                        float scale = Math.Min(
                            listViewItemIconContainer.Width / listViewItemIcon.Texture.Width,
                            listViewItemIconContainer.Height / listViewItemIcon.Texture.Height
                        );
                        listViewItemIcon.ResizeTo(listViewItemIcon.Texture.Size * scale);
                        listViewItemIcon.FadeIn();
                        listViewItemIconContainer.FadeIn();
                    }
                    else
                    {
                        listViewItemIconContainer.FadeOut();
                    }
                }
            }
        }
    }


}
