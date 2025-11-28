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
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSTreeViewNode: TreeView.DrawableTreeViewNode
    {
        #region Members
        private KCSTreeViewNodeTextContainer text;
        #endregion
        #region Properies

        #endregion
        #region Constructors
        public KCSTreeViewNode(int depth, TreeViewNode item) : base(depth, item)
        {
            Margin = new MarginPadding { Vertical = 2 };
        }
        #endregion

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Colour4.Transparent;
            BackgroundHoverColour = Colour4.FromHex("1A1A1A");
            BackgroundSelectedColour = Colour4.FromHex("134B70");
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

        protected virtual KCSTreeViewNodeTextContainer CreateTextContainer() => new KCSTreeViewNodeTextContainer();

        protected partial class KCSTreeViewNodeTextContainer : Container, IHasText, IHasIcon
        {
            private readonly Container TreeViewNodeIconContainer;
            private readonly Sprite TreeViewNodeIcon;
            private readonly SpriteText TreeViewNodeText;
            private TextureStore textureStore;
            private string iconTextureName = "";

            private LocalisableString text;

            public LocalisableString Text
            {
                get => text;
                set
                {
                    text = value;
                    TreeViewNodeText.Text = value;
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

            public KCSTreeViewNodeTextContainer()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
                Child = new FillFlowContainer()
                {
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.X,
                    Height = 25f,
                    Children = new Drawable[]
                    {
                        TreeViewNodeIconContainer = new Container()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.None,
                            Width = 16f,
                            Height = 16f,
                            Alpha = 0f,
                            Child = TreeViewNodeIcon = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.None,
                                TextureRelativeSizeAxes = Axes.Both
                            },
                            Margin = new MarginPadding() { Right = 7 }
                        },
                        TreeViewNodeText = new SpriteText()
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

            [BackgroundDependencyLoader]
            private void load(TextureStore textureStore)
            {
                this.textureStore = textureStore;
                loadIcon();
            }

            private void loadIcon()
            {
                if (textureStore is not null && iconTextureName.Length > 0 && TreeViewNodeIcon is not null)
                {
                    TreeViewNodeIcon.Texture = textureStore.Get(iconTextureName);
                    if (TreeViewNodeIcon.Texture is not null)
                    {
                        float scale = Math.Min(
                            TreeViewNodeIconContainer.Width / TreeViewNodeIcon.Texture.Width,
                            TreeViewNodeIconContainer.Height / TreeViewNodeIcon.Texture.Height
                        );
                        TreeViewNodeIcon.ResizeTo(TreeViewNodeIcon.Texture.Size * scale);
                        Logger.Log($"{TreeViewNodeIcon.Texture.Size * scale}");
                        TreeViewNodeIconContainer.FadeIn();
                    }
                }
            }
        }
    }
}
