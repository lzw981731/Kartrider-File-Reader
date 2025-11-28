using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace KartCityStudio.Game.Graphics.Sprites;

public interface IHasIcon: IDrawable
{
    string IconTextureName  { get; set; }
}
