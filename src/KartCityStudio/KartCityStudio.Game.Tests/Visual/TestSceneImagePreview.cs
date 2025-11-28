using KartCityStudio.Game.Graphics.UserInterface;
using KartLibrary.File;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneImagePreview : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        private KCSImagePreview imagePreview;

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        public TestSceneImagePreview()
        {
            //Add(mainScrollBar = new KCSScrollBar());
            // gamania logo: zeta_/tw/logo/publisher/gamania_logo00.png
            // korea warning logo: zeta/kr/logo/warning/폭럭성.png
            // Add(imagePreview = new KCSImagePreview("zeta/kr/logo/warning/폭력성.png")
            // {
            //     RelativeSizeAxes = Axes.Both
            // });
        }
    }
}
