using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK;
namespace KartCityStudio.Game.Graphics.Containers
{
    public partial class KCSScrollContainer<T> : ScrollContainer<T> where T : Drawable
    {
        private readonly KCSScrollBar scrollBar;
        private readonly Container scrollBase;
        private readonly FlowContainer<Drawable> scrollContent;
        private bool isHover = false;

        public bool AutoHideScrollerBar { get; set; } = false;

        public float ScrollerSize { get; set; } = 4f;

        public KCSScrollContainer(Direction direction = Direction.Vertical): base(direction)
        {
            ClampExtension = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {

        }

        protected override ScrollbarContainer CreateScrollbar(Direction direction)
        {
            return new KCSScrollBar(direction)
            {
                Size = direction == Direction.Vertical ? new Vector2(ScrollerSize, 1.0f) : new Vector2(1.0f, ScrollerSize),
                Anchor = direction == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft,
                Origin = direction == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            isHover = true;
            Scheduler.AddOnce(updateScrollerVisibility);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            isHover = false;
            Scheduler.AddOnce(updateScrollerVisibility);
            base.OnHoverLost(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            Logger.Log($"Drag.");
            base.OnDrag(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Logger.Log($"MouseDown");
            return base.OnMouseDown(e);
        }

        private void updateScrollerVisibility()
        {
            if (AutoHideScrollerBar && Scrollbar is KCSScrollBar scrollbar)
            {
                bool isDragging = scrollbar.IsDragging;
                if (!isDragging && !isHover)
                {
                    Scrollbar
                        .Delay(300)
                        .FadeOut(duration: 500, easing: Easing.OutExpo);
                }
                else if (isHover || isDragging)
                {
                    Scrollbar
                        .FadeIn(duration: 239, easing: Easing.InOutQuint);
                }
            }
        }

        protected partial class KCSScrollBar : ScrollbarContainer
        {
            private readonly Box scroller;
            private float scrollerHideAlpha = 0.2f;
            private bool isHover = false;
            private bool isDragging = false;

            public bool IsDragging => isDragging;

            public KCSScrollBar(Direction direction): base(direction)
            {
                Anchor = direction == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft;
                Origin = direction == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft;
                Blending = BlendingParameters.Additive;
                Children = new Drawable[]
                {
                    scroller = new Box()
                    {
                        Alpha = scrollerHideAlpha,
                        RelativePositionAxes = Axes.Both,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = direction == Direction.Vertical ? Anchor.TopCentre : Anchor.CentreLeft,
                        Origin = direction == Direction.Vertical ? Anchor.TopCentre : Anchor.CentreLeft,
                        Size = new Vector2(1f, 1f),
                        Position = new Vector2(0f, 0f),
                        Colour = Colour4.White,
                    },
                };
                Masking = true;
                CornerRadius = 5f;
            }

            protected override void Update()
            {
                base.Update();
                Anchor =  ScrollDirection == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft;
                Origin = ScrollDirection == Direction.Vertical ? Anchor.TopRight : Anchor.TopLeft;
            }

            protected override bool OnHover(HoverEvent e)
            {
                isHover = true;
                scroller
                    .FadeTo(1, duration: 239, easing: Easing.InOutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                isHover = false;
                scroller
                    .FadeTo(scrollerHideAlpha, duration: 500, easing: Easing.OutExpo);
                base.OnHoverLost(e);
            }

            protected override bool OnScroll(ScrollEvent e)
            {
                scroller
                    .FadeTo(1, duration: 239, easing: Easing.InOutQuint);
                return base.OnScroll(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                isDragging = true;
                return base.OnDragStart(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                isDragging = false;
                base.OnDragEnd(e);
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                this.ResizeTo(new Vector2(this.Size[(int)ScrollDirection ^ 1])
                {
                    [(int)ScrollDirection] = val
                }, 320, Easing.OutExpo);
            }
        }
    }
}
