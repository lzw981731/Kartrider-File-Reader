using KartCityStudio.Game.Graphics.UserInterface;
using osu.Framework.Graphics;

namespace KartCityStudio.Game.Graphics.Containers;

public partial class KCSTabControlContainer: TabControlContainer
{
    protected override TabItemListBox CreateTabItemListBox() => new KCSTabItemListBox()
    {
        RelativeSizeAxes = Axes.X,
        Height = 30,
        BackgroundColour = Colour4.FromHex("000000"),
    };
}
