using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.UserInterface;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using TabItem = KartCityStudio.Game.Graphics.UserInterface.TabItem;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneTabControlContainer : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSTabControlContainer tabControlContainer;
        public TestSceneTabControlContainer()
        {
            Add(tabControlContainer = new KCSTabControlContainer()
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Add a tab", () =>
            {
                tabControlContainer.Add(new TabPageContainer($"TestPage {tabControlContainer.Count}")
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new BasicButton()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new osuTK.Vector2(1, 1),
                        Text = $"It is TestPage {tabControlContainer.Count}!",
                        Position = new osuTK.Vector2(0, 0)
                    }
                });
            });
        }
    }
}
