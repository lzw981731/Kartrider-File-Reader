using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface;

public partial class KCSLoadingSpinner: VisibilityContainer
{
    private readonly Container background;
    private readonly Box backgroundBox;
    private readonly Container rotatingCircleContainer;
    private readonly CircularProgress rotatingCircle;

    public KCSLoadingSpinner()
    {
        AlwaysPresent = true;
        InternalChildren = new Drawable[]
        {
            background = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 50f,
                Scale = new Vector2(0),
                Child = backgroundBox = new Box()
                {
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.FromHex("3030301A"),
                }
            },
            rotatingCircleContainer = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0),
                Child = rotatingCircle = new CircularProgress()
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.75f),
                    Colour = Colour4.White,
                    InnerRadius = 0.3f,
                    Progress = 0.1,
                    RoundedCaps = true
                }
            },
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {

    }

    protected override void Update()
    {
        background.CornerRadius = DrawSize.X / 2f;
        base.Update();
    }

    protected override void PopIn()
    {
        rotatingCircle.ClearTransforms();
        rotatingCircleContainer.ClearTransforms();
        background.ClearTransforms();
        backgroundBox.ClearTransforms();

        rotatingCircle
            .ResizeTo(new Vector2(.75f, .75f))
            .RotateTo(0)
            .FadeIn(500, Easing.OutQuint)
            .ProgressTo(0.73, 500, Easing.InOutQuint);

        rotatingCircleContainer
            .ScaleTo(1, 500, Easing.OutQuint)
            .Spin(1013, RotationDirection.Clockwise);

        background
            .FadeIn(500, Easing.OutQuint)
            .ScaleTo(1, 500, Easing.OutQuint);

        backgroundBox
            .FadeColour(Colour4.FromHex("3030309F"), 1109, Easing.OutQuint)
            .Then()
            .FadeColour(Colour4.FromHex("3030301A"), 1217, Easing.OutQuint)
            .Then()
            .Loop();
    }

    protected override void PopOut()
    {
        rotatingCircle.ClearTransforms();
        background.ClearTransforms();
        backgroundBox.ClearTransforms();

        rotatingCircle
            .ResizeTo(new Vector2(-.75f, .75f))
            .RotateTo(-360 * (float)(1 - rotatingCircle.Progress))
            .ProgressTo(0, 456, Easing.OutQuint)
            .FadeOut(456, Easing.OutQuint);

        background.FadeOut(456, Easing.OutQuint);
    }
}
