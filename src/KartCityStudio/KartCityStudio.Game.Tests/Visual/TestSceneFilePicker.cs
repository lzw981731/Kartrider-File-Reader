using System.Text.RegularExpressions;
using KartCityStudio.Game.Graphics.UserInterface;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneFilePicker : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSFilePicker filePicker;
        public TestSceneFilePicker()
        {
            Add(filePicker = new KCSFilePicker()
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Clear file name filter", () =>
            {
                filePicker.FileNameFilter = null;
            });

            AddStep("Change file name filter to rho|rho5.", () =>
            {
                filePicker.FileNameFilter = new Regex(@".*\.(rho|rho5)");
            });

            AddStep("Change file name filter to dll|exe", () =>
            {
                filePicker.FileNameFilter = new Regex(@".*\.(dll|exe)");
            });
        }
    }
}
