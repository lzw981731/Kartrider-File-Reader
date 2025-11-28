using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneXmlView : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSXmlView xmlView;

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        public TestSceneXmlView()
        {

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
                Add(xmlView = new KCSXmlView()
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1, 1),
                    BackgroundColour = Colour4.Black
                });
                BinaryXmlTag localeXml = bgmLocaleFile.ReadXml();
                xmlView.LoadXml(localeXml);
            }
        }
    }
}
