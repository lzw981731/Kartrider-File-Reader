using System.Linq;
using KartCity.Common.FileType;
using KartCity.Common.Xml;
using KartCityStudio.Game.Extension;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.UserInterface;
using KartCityStudio.Game.Text;
using KartLibrary.File;
using KartLibrary.Xml;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneKCSFormattableText : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        //private KCSScrollBar mainScrollBar;
        private readonly KCSFormattableText formattableText;

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        public TestSceneKCSFormattableText()
        {

            //Add(mainScrollBar = new KCSScrollBar());
            Add(new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f),
                Colour = Colour4.FromHex("191919")
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            KartStorageFile? bgmLocaleFile = storageSystem?.GetFile("etc_/bgmList.xml")
                                             ?? storageSystem?.GetFile("etc_/bgmList.bml")
                                             ?? storageSystem?.GetFile("etc/bgmList.xml")
                                             ?? storageSystem?.GetFile("etc/bgmList.bml");
            if (bgmLocaleFile is not null)
            {
                BinaryXmlTag localeXml = bgmLocaleFile.ReadXml();
                string formatXml = localeXml.ToFormattableText();
                float offset = 20f;
                FormattableTextParser parser = new FormattableTextParser();
                var parseResult = parser.Parse(formatXml);
                var scrollContainer = new KCSScrollContainer<KCSFormattableText>(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1f, 1f),
                };

                int i = 0;
                foreach (var parseResultItem in parseResult)
                {
                    scrollContainer.Add(new KCSFormattableText(parser)
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.5f,
                        Height = 20f,
                        StylizedTexts = parseResultItem,
                        Y = i * 20f,
                        Colour = Colour4.White
                    });
                    i++;
                }

                Add(scrollContainer);
            }
        }
    }
}
