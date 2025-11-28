using KartCityStudio.Game.Graphics.UserInterface;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneLoadingSpinner : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        //private KCSScrollBar mainScrollBar;
        private KCSLoadingSpinner loadingSpinner;
        public TestSceneLoadingSpinner()
        {
            //Add(mainScrollBar = new KCSScrollBar());
            Add(loadingSpinner = new KCSLoadingSpinner()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(70, 70)
            });

            AddStep("Pop in", () => loadingSpinner.Show());

            AddStep("Pop out", () => loadingSpinner.Hide());
        }
    }
}
