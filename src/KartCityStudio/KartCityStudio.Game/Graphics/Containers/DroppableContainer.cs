using System;
using KartCityStudio.Game.Input.Events;
using KartCityStudio.Game.Service;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace KartCityStudio.Game.Graphics.Containers;

public partial class DroppableContainer: Container
{
    private readonly Container<Drawable> contentContainer;

    protected override Container<Drawable> Content => contentContainer;

    public event Action<DragDropEvent>? DragDrop;

    private DragDropEventChainHandle dropEventChainHandle;

    [Resolved]
    private WindowEventService windowEventService { get; set; }

    public DroppableContainer()
    {
        InternalChildren = new[]
        {
            contentContainer = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        dropEventChainHandle = windowEventService.RegisterDropDragEvent(dragDropCallback);
    }

    protected override void Dispose(bool isDisposing)
    {
        if(dropEventChainHandle is not null)
            windowEventService.UnregisterDropDragEvent(dropEventChainHandle);
        base.Dispose(isDisposing);
    }

    protected virtual void OnDragDrop(DragDropEvent e)
    {

    }

    private bool dragDropCallback(DragDropEvent e)
    {
        DragDrop?.Invoke(e);
        OnDragDrop(e);
        return true;
    }
}
