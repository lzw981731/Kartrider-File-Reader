using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using KartCityStudio.Game.IO.Stores;
using KartCityStudio.Game.Model;
using KartLibrary.File;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace KartCityStudio.Game.Graphics.UserInterface;

public partial class KCSImagePreview: Container
{
    private readonly Box background;
    private readonly Sprite imageViewSprite;
    private readonly KCSIconButton scaleUpButton;
    private readonly KCSIconButton scaleDownButton;
    private readonly KCSButton scaleButton;
    private readonly KCSLoadingSpinner loadingSpinner;
    private readonly Box scaleProgressBox;
    private readonly SpriteText scaleText;

    private float scaleRate;

    private Texture? texture;
    private Vector2 dragOffset;

    private BackgroundWorker bgLoadImageWorker;

    private IArchiveFile baseImageFile;

    public float ScaleRate
    {
        get => scaleRate;
        set
        {
            scaleRate = value;
            Scheduler.AddOnce(scaleImage);
        }
    }


    public Colour4 BackgroundColour
    {
        get => background.Colour;
        set => background.Colour = value;
    }

    public KCSImagePreview(IArchiveFile imageFile)
    {
        this.baseImageFile = imageFile;

        InternalChildren = new Drawable[]
        {
            background = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.FromHex("121212"),
            },
            imageViewSprite = new Sprite()
            {
                RelativeSizeAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            },
            scaleUpButton = new KCSIconButton()
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(42f, 42f),
                Position = new Vector2(-30, -30),
                Masking = true,
                CornerRadius = 21,
                Alpha = 0.8f,
                BackgroundColour = Colour4.FromHex("000000"),
                HoverColour = Colour4.FromHex("FFFFFF3A"),
                Icon = FontAwesome.Solid.Plus,
                IconRelativeSize = new Vector2(0.3f),
                Action = scaleUp
            },
            scaleButton = new KCSButton()
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(180f, 42f),
                Position = new Vector2(-85, -30),
                Masking = true,
                CornerRadius = 21,
                Alpha = 0.8f,
                BackgroundColour = Colour4.FromHex("000000"),
                HoverColour = Colour4.FromHex("FFFFFF3A"),
                Action = resetScale,
                Children = new Drawable[]
                {
                    scaleProgressBox = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(1f, 1f),
                        Scale = new Vector2(0f, 1f),
                        Colour = Colour4.FromHex("FFFFFF4F")
                    },
                    scaleText = new SpriteText()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.X,
                        Text = "100.00%",
                        Font = KCSFont.DefaultM,
                        Colour = Colour4.FromHex("FFFFFF"),
                    },
                }
            },
            scaleDownButton = new KCSIconButton()
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(42f, 42f),
                Position = new Vector2(-278, -30),
                Masking = true,
                CornerRadius = 21,
                Alpha = 0.8f,
                BackgroundColour = Colour4.FromHex("000000"),
                HoverColour = Colour4.FromHex("FFFFFF3A"),
                Icon = FontAwesome.Solid.Minus,
                IconRelativeSize = new Vector2(0.3f),
                Action = scaleDown
            },
            loadingSpinner = new KCSLoadingSpinner()
            {
                RelativeSizeAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(70)
            }
        };

        bgLoadImageWorker = new BackgroundWorker();
        bgLoadImageWorker.DoWork += bgLoadImageWork;
        bgLoadImageWorker.RunWorkerCompleted += bgLoadImageWorkFinished;
    }

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        bgLoadImageWorker.RunWorkerAsync(host);
    }

    private void bgLoadImageWork(object? sender, DoWorkEventArgs e)
    {
        Scheduler.AddOnce(loadingSpinner.Show);
        if (e.Argument is GameHost host)
        {
            using Stream textureStream = this.baseImageFile.CreateStream();
            Texture newTexture = Texture.FromStream(host.Renderer, textureStream);
            e.Result = newTexture;
        }
    }

    private void bgLoadImageWorkFinished(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error is not null)
        {
            Scheduler.AddOnce(loadingSpinner.Hide);
        }
        else if(e.Result is Texture newTexture)
        {
            Scheduler.Add(() =>
            {
                imageViewSprite.Texture = texture = newTexture;
                imageViewSprite.Scale = new Vector2(1f);
                imageViewSprite.ClearTransforms();
                imageViewSprite
                    .MoveTo(new Vector2())
                    .FadeIn(350, Easing.OutQuint);
                scaleRate = 1f;
                updateSize();
                Scheduler.AddOnce(loadingSpinner.Hide);
            });
        }
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        ScaleRate = ScaleRate * MathF.Pow(1.1f, e.ScrollDelta.Y);
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if(e.Button != osuTK.Input.MouseButton.Left) return false;

        dragOffset = Position;

        return true;
    }

    protected override bool OnDragStart(DragStartEvent e)
    {
        if (e.Button != osuTK.Input.MouseButton.Left) return false;

        dragOffset = e.MousePosition - imageViewSprite.Position;

        return true;
    }

    protected override void OnDrag(DragEvent e)
    {
        if (texture is not null)
        {
            Vector2 newPos = e.MousePosition - dragOffset;
            imageViewSprite.Position = newPos;
        }
    }

    protected override void Update()
    {
        if (texture is not null)
        {
            Vector2 pos = imageViewSprite.Position;
            Vector2 moveRange = imageViewSprite.Size - this.DrawSize;
            moveRange.X = MathF.Max(moveRange.X, 0) / 2f;
            moveRange.Y = MathF.Max(moveRange.Y, 0) / 2f;
            pos.X = Math.Clamp(pos.X, -moveRange.X, moveRange.X);
            pos.Y = Math.Clamp(pos.Y, -moveRange.Y, moveRange.Y);
            imageViewSprite.Position = pos;

            float currentScale = (texture.Size.X > 0 ? imageViewSprite.Size.X / texture.Size.X : 1f) * 100f;
            scaleText.Text = $"{currentScale:0.00}%";
        }
        base.Update();
    }

    private void updateSize()
    {
        Vector2 textureSize = texture.Size;
        imageViewSprite.Size = textureSize;
    }

    private void scaleImage()
    {
        if (texture is not null)
        {
            scaleRate = Math.Clamp(scaleRate, 1f, 20f);
            imageViewSprite.ClearTransforms();
            scaleProgressBox.ClearTransforms();
            scaleButton.ClearTransforms();
            imageViewSprite.ResizeTo(texture.Size * scaleRate, 620, Easing.OutExpo);
            scaleProgressBox.ScaleTo(new Vector2((scaleRate - 1) / 19f, 1f), 620, Easing.OutExpo);
        }
    }

    private void scaleUp()
    {
        ScaleRate *= 1.21f;
    }

    private void scaleDown()
    {
        ScaleRate /= 1.21f;
    }

    private void resetScale()
    {
        ScaleRate = 1f;
    }
}
