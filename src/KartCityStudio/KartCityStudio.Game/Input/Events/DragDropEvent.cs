using osu.Framework.Input.Events;
using osu.Framework.Input.States;

namespace KartCityStudio.Game.Input.Events;

public class DragDropEvent: UIEvent
{
    public string Path { get; init; }

    public DragDropEvent(InputState state, string path) : base(state)
    {
        Path = path;
    }
}
