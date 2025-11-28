using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using Veldrid;

namespace KartCityStudio.Game.Graphics.Containers;

public partial class DialogContainer: VisibilityContainer
{
    private readonly DialogBackgroundMask background;
    private readonly Container<Drawable> contentContainer;

    protected override Container<Drawable> Content => contentContainer;

    public DialogContainer()
    {
        InternalChildren = new Drawable[]
        {
            background = new DialogBackgroundMask()
            {
                Alpha = 0f,
                AlwaysPresent = false,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.FromHex("000000AF"),
            },
            contentContainer = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                // BlurSigma = new Vector2(12f),
                Scale = new Vector2(0.95f)
            }
        };
        background.MouseDown += BackgroundOnMouseDown;
        background.MouseUp += BackgroundOnMouseUp;
    }

    private void BackgroundOnMouseUp(MouseUpEvent obj)
    {
        // if (this.State.Value == Visibility.Visible)
        // {
        //     contentContainer
        //         .ScaleTo(new Vector2(1f), 770, Easing.OutElasticHalf);
        // }
    }

    private void BackgroundOnMouseDown(MouseDownEvent obj)
    {
        // if (this.State.Value == Visibility.Visible)
        // {
        //     contentContainer
        //         .ScaleTo(new Vector2(1.02f), 600, Easing.OutQuint);
        // }
    }

    protected override void PopIn()
    {
        contentContainer
            .FadeIn(300)
            .ScaleTo(new Vector2(1.0f), 570, Easing.OutElasticQuarter)
            // .BlurTo(new Vector2(0), 570, Easing.OutQuint)
            ;
        background.FadeIn(370, Easing.OutQuint);
        // contentContainer.BlurTo(new Vector2(0f), 540, Easing.OutElastic);
    }

    protected override void PopOut()
    {
        contentContainer
            .FadeOut(570, Easing.OutQuint)
            .ScaleTo(new Vector2(0.95f), 570, Easing.OutQuart)
            // .BlurTo(new Vector2(12f), 570, Easing.OutQuint)
            ;
        background.FadeOut(570, Easing.OutQuart);
        // contentContainer.BlurTo(new Vector2(1000f), 540, Easing.OutQuint);
    }

    private partial class DialogBackgroundMask: Box
    {
        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            MouseDown?.Invoke(e);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            MouseUp?.Invoke(e);
        }

        protected override bool OnHover(HoverEvent e) => true;
        protected override bool OnScroll(ScrollEvent e) => true;
        protected override bool OnDoubleClick(DoubleClickEvent e) => true;
        protected override bool OnDragStart(DragStartEvent e) => true;
        protected override bool OnMouseMove(MouseMoveEvent e) => true;

        public event Action<MouseDownEvent>? MouseDown;
        public event Action<MouseUpEvent>? MouseUp;
    }
}

