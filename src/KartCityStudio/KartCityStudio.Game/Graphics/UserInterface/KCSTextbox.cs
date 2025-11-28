using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSTextbox : TextBox
    {
        private readonly Container contentContainer = new Container()
        {
            RelativeSizeAxes = Axes.Both,
            Margin = new MarginPadding() { Right = -10f },
            Masking = true,
        };

        private readonly Box background;
        private readonly Container innerContainer;

        protected KCSCaret Caret;

        public Colour4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public new float CornerRadius
        {
            get => innerContainer.CornerRadius;
            set => innerContainer.CornerRadius = value;
        }

        protected override Container Content => contentContainer;

        protected override Caret CreateCaret() => Caret = new KCSCaret();

        protected override SpriteText CreatePlaceholder() => new SpriteText()
        {
            Font = KCSFont.Default,
            Margin = new MarginPadding { Left = 2 },
        };

        protected override void NotifyInputError()
        {

        }

        protected override float LeftRightPadding => 10f;

        protected override Drawable GetDrawableCharacter(char c) => new SpriteText
        {
            Text = c.ToString(),
            Font = KCSFont.Default,
        };

        public Regex? NameFilter { get; set; } = null;

        public KCSTextbox()
        {
            Masking = true;
            InternalChildren = new Drawable[]
            {
                innerContainer = new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        contentContainer,
                        background = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Depth = 1,
                            Colour = Colour4.Transparent,
                        }
                    },
                },

            };
            TextContainer.Height = 0.8f;
            TextContainer.Masking = true;
            BackgroundColour = Colour4.FromHex("2A2A2A");
        }

        protected partial class KCSCaret: Caret
        {
            private readonly Box background;
            private readonly Container maskingContainer;

            public float CaretWidth { get; set; } = 3f;

            public Colour4 SelectionColour { get; set; } = Colour4.FromHex("3887BE");

            public float FlickingDuration = 600;

            public KCSCaret()
            {
                Height = 0.75f;
                InternalChild = maskingContainer = new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 3,
                    Child = background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White,
                        Alpha = 0.6f,
                    }
                };
            }

            // Refers to OsuTextBox.
            public override void DisplayAt(Vector2 position, float? selectionWidth)
            {
                this.ClearTransforms();

                if(selectionWidth is not null)
                {
                    this.MoveTo(position, 60, Easing.Out);
                    this.ResizeWidthTo(selectionWidth.Value, 60, Easing.Out);
                    this.FadeColour(SelectionColour, 200, Easing.Out);
                }
                else
                {
                    this.MoveTo(new Vector2(position.X - CaretWidth / 2, position.Y), 60, Easing.Out);
                    this.ResizeWidthTo(CaretWidth, 60, Easing.Out);
                    this.FadeColour(Colour4.White, 200, Easing.Out);
                    this.FadeIn().Delay(FlickingDuration).FadeTo(0f).Delay(FlickingDuration).Loop();
                }
            }
        }
    }
}
