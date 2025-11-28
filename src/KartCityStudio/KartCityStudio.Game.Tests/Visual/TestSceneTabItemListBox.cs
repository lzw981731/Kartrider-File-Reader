using System.Threading.Tasks;
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
    public partial class TestSceneTabItemListBox : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private TabItemListBox listbox;
        public TestSceneTabItemListBox()
        {
            Add(listbox = new KCSTabItemListBox()
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1f, 30f),
                BackgroundColour = Colour4.FromHex("181818"),
            });
            AddStep("Add 10 items to ListBox.", () =>
            {
                for (int i = 0; i < 10; i++)
                    listbox.Items.Add(new TabItem($"TestItem{listbox.Items.Count + 1}", closeAction: tabCloseAction));
            });
            AddStep("Hide TestItem1.", () =>
            {

            });
            listbox.Items.Add(new TabItem("TestItem1!", closeAction: tabCloseAction));
            listbox.Items.Add(new TabItem("TestItem2!", closeAction: tabCloseAction));
        }

        private void tabCloseAction(TabItem obj)
        {
            listbox.Items.Remove(obj);
        }
    }
}
