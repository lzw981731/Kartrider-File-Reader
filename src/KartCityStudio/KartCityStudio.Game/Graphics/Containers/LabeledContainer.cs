using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace KartCityStudio.Game.Graphics.Containers;

public partial class LabeledContainer: Container
{
    private readonly SpriteText labelText;
    private readonly Box labelBackground;
    private readonly Box labelBottomLine;
    private readonly Container contentContainer;

    protected override Container<Drawable> Content => contentContainer;

    public LocalisableString Label
    {
        get => labelText.Text;
        set => labelText.Text = value;
    }

    public LabeledContainer()
    {
        InternalChildren = new Drawable[]
        {
            contentContainer = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Padding = new MarginPadding() { Top = 30 }
            },
            new Container()
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Children = new Drawable[]
                {
                    labelBackground = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("09090C"),
                    },
                    labelText = new SpriteText()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Colour = Colour4.White,
                        Font = KCSFont.Default,
                        Margin = new MarginPadding() { Left = 15 }
                    },
                    labelBottomLine = new Box()
                    {
                        RelativeSizeAxes = Axes.None,
                        Colour = Colour4.FromHex("295F98"),
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding(){Horizontal = 15f},
                        Size = new Vector2(0f, 2f)
                    }
                }
            },
        };
    }

    protected override void Update()
    {
        base.Update();
        labelBottomLine.Width = DrawWidth - 30;
    }
}
