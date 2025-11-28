using System;
using System.ComponentModel;
using System.IO;
using KartCity.Common.FileType;
using KartLibrary.File;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osuTK;

namespace KartCityStudio.Game.Graphics.Sprites;

public partial class RaycitySprite: CompositeDrawable
{
    private readonly Vector2 raycityBackgroundSize = new Vector2(1024, 768);

    private Sprite imageSprite;

    private BackgroundWorker bgLoadImageWorker;

    private IRhoFile baseImageFile;

    private Texture texture;

    public RaycitySprite(IRhoFile imageFile)
    {
        this.baseImageFile = imageFile;
        InternalChildren = new Drawable[]
        {
            imageSprite = new Sprite()
            {
                RelativeSizeAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
        if (e.Argument is GameHost host)
        {
            using Stream textureStream = this.baseImageFile.CreateStream();
            Texture newTexture = Texture.FromStream(host.Renderer, textureStream);
            newTexture = newTexture?.Crop(cropRectangle: new RectangleF(new Vector2(0f), raycityBackgroundSize));
            e.Result = newTexture;
        }
    }

    private void bgLoadImageWorkFinished(object? sender, RunWorkerCompletedEventArgs e)
    {
        if(e.Result is Texture newTexture)
        {
            Scheduler.Add(() =>
            {
                imageSprite.Texture = texture = newTexture;
                imageSprite.Scale = new Vector2(1f);
                imageSprite.ClearTransforms();
                imageSprite
                    .MoveTo(new Vector2())
                    .FadeIn(350, Easing.OutQuint);
                updateSize();
            });
        }
    }

    private void updateSize()
    {
        float widthScale = this.DrawSize.X / texture.Width;
        float heightScale = this.DrawSize.Y / texture.Height;
        float scale = MathF.Max(widthScale, heightScale);
        imageSprite.Scale = new Vector2(scale);
    }

    protected override void UpdateAfterAutoSize()
    {
        base.UpdateAfterAutoSize();
        if(texture is not null)
            updateSize();
    }
}
