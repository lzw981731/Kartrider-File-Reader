using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface;

public partial class KCSMessageBox: CompositeDrawable
{
    private readonly Sprite messageIcon;

    private readonly SpriteText messageTitle;

    private readonly Container messageContainer;

    private readonly SpriteText optionText;

    private readonly Container optionBgContainer;

    private readonly Box optionBackground;

    private readonly Box background;

    public KCSMessageBox()
    {
        InternalChildren = new Drawable[]
        {
            background = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f),
                Colour = Colour4.FromHex("111111"),
            },
            messageContainer = new Container()
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Position = new Vector2(0, 50f),
                Children = new Drawable[]
                {
                    messageIcon = new Sprite()
                    {
                        RelativePositionAxes = Axes.None,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Scale = new Vector2(0.3f),
                        Position = new Vector2(0),
                    },
                    messageTitle = new SpriteText()
                    {
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Text = "Can't load this file because testing.",
                        Font = KCSFont.RubikSemiBold.With(size: 25),
                        Position = new Vector2(0),
                    },
                }
            },
            optionBgContainer = new Container()
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.TopCentre,
                RelativeSizeAxes = Axes.None,
                RelativePositionAxes = Axes.None,
                Position = new Vector2(0, 223f),
                Size = new Vector2(300f, 36f),
                CornerRadius = 8f,
                Masking = true,
                Child = optionBackground = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1f),
                    Colour = Colour4.FromHex("#FFFF007A"),
                    EdgeSmoothness = Vector2.One,
                },
            },

            optionText = new SpriteText()
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.TopCentre,
                Text = "OK",
                Font = KCSFont.RubikSemiBold.With(size: 22),
                Position = new Vector2(0, 223f),
                Colour = Colour4.White,
            },

        };
    }

    [BackgroundDependencyLoader]
    private void load(TextureStore textures)
    {
        Vector2 msgIconOffset = new Vector2(0, 30);
        Vector2 msgTextOffset = new Vector2(0, 60);
        Vector2 msgContainerOffset = new Vector2(0, 20);
        Vector2 msgOption = new Vector2(0, 20);
        Vector2 msgOptionBg = new Vector2(0, 30);
        messageIcon.Texture = textures.Get("exclamation-triangle-fill");
        float aniTime = 900;
        messageContainer
            .MoveToOffset(msgContainerOffset, aniTime, Easing.OutExpo);
        messageIcon
            .FadeInFromZero(aniTime, Easing.OutExpo)
            .MoveToOffset(-msgIconOffset)
            .MoveToOffset(msgIconOffset, aniTime, Easing.OutExpo)
            ;
        messageTitle
            .FadeInFromZero(aniTime, Easing.OutExpo)
            .MoveToOffset(-msgTextOffset)
            .MoveToOffset(msgTextOffset, aniTime, Easing.OutExpo)
            ;
        optionText.MoveToOffset(-msgOption)
            .FadeTo(0)
            .Delay(aniTime * 0.1f)
            .FadeInFromZero(aniTime, Easing.OutExpo)
            .MoveToOffset(msgOption, aniTime, Easing.OutExpo);
        optionBgContainer.MoveToOffset(-msgOptionBg)
            .FadeTo(0)
            .Delay(aniTime * 0.2f)
            .FadeInFromZero(aniTime, Easing.OutExpo)
            .ResizeHeightTo(0)
            .ResizeHeightTo(30, aniTime, Easing.OutExpo)
            .MoveToOffset(msgOptionBg, aniTime, Easing.OutExpo);
    }


}
