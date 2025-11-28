using KartCityStudio.Game.Graphics.UserInterface;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneButton : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        //private KCSScrollBar mainScrollBar;
        private KCSButton button;

        private SpriteText text;

        public TestSceneButton()
        {
            //Add(mainScrollBar = new KCSScrollBar());
            
            Add(button = new KCSButton
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.None,
                RelativePositionAxes = Axes.None,
                Position = new osuTK.Vector2(30, 30),
                Size = new osuTK.Vector2(70, 35),
                Child = text = new SpriteText()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(3),
                    Text = "ACCCCCCCCCCCCCCCCCCCCC\n\nACCC"
                }
            });
        }
    }
}
