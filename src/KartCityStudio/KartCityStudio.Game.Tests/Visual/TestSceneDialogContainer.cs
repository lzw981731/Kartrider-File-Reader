using System.Text.RegularExpressions;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.UserInterface;
using KartCityStudio.Game.Screens;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneDialogContainer : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSFilePicker filePicker;
        private DialogContainer dialogContainer;
        public TestSceneDialogContainer()
        {
            BufferedContainer bufferedContainer = new BufferedContainer();
            Add(new ScreenStack(new MainScreen()) { RelativeSizeAxes = Axes.Both });
            Add(dialogContainer = new DialogContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.9f),
                    Child = filePicker = new KCSFilePicker()
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColour = new ColourInfo()
                        {
                            TopLeft = Colour4.FromHex("A67B5B") *  Colour4.FromHex("777777"),
                            TopRight = Colour4.FromHex("A67B5B")*  Colour4.FromHex("999999"),
                            BottomLeft = Colour4.FromHex("A67B5B")*  Colour4.FromHex("BBBBBB"),
                            BottomRight = Colour4.FromHex("A67B5B")*  Colour4.FromHex("CCCCCC")
                        }
                    },
                }
            });
            AddLabel("Pop In / Out");
            AddStep("Pop In", () =>
            {
                dialogContainer.Show();
            });
            AddWaitStep("Waiting for popping in.", 5);
            AddStep("Pop Out", () =>
            {
                dialogContainer.Hide();
            });
        }
    }
}
