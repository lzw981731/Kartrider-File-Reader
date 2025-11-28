using KartCityStudio.Game.Graphics.UserInterface;
using NUnit.Framework;
using osu.Framework.Graphics;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneMessageBox : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        //private KCSScrollBar mainScrollBar;
        private readonly KCSMessageBox messageBox;
        public TestSceneMessageBox()
        {
            //Add(mainScrollBar = new KCSScrollBar());
            Add(messageBox = new KCSMessageBox()
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
                Height = 300f,
            });
        }
    }
}
