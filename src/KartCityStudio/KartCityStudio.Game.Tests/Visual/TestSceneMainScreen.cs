using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.UserInterface;
using KartCityStudio.Game.Screens;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.Game.Localization;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneMainScreen : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        private MainScreen screen;

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        private KCSMusicPlayer musicPlayer;

        private Container musicPlayerContainer;

        private KCSToolboxGroup bgmListToolboxGroup;

        private KCSListBox bgmListBox;

        private StringBag trackStringBag;

        private Dictionary<string, string> trackPaths;

        private CountryCode activeCountryCode;

        public TestSceneMainScreen()
        {
            Add(new ScreenStack(screen = new MainScreen()) { RelativeSizeAxes = Axes.Both });

            AddWaitStep("1000", 19);

            AddStep("Set region to Korea", () =>
            {
                activeCountryCode = CountryCode.KR;
                updateBgmList();
            });

            AddStep("Set region to China", () =>
            {
                activeCountryCode = CountryCode.CN;
                updateBgmList();
            });

            AddStep("Set region to Taiwan", () =>
            {
                activeCountryCode = CountryCode.TW;
                updateBgmList();
            });

            AddStep("Show bgm list", () =>
            {
                bgmListToolboxGroup.FadeIn(320, Easing.OutQuint);
            });

            AddStep("Hide bgm list", () =>
            {
                bgmListToolboxGroup.FadeOut(320, Easing.OutQuint);
            });

            AddStep("Add tab", () =>
            {
                TabPageContainer pageContainer = new TabPageContainer("Music player")
                {
                    RelativeSizeAxes = Axes.Both
                };

                activeCountryCode = CountryCode.KR;
                trackStringBag = new StringBag();
                trackPaths = new Dictionary<string, string>();

                pageContainer.Add(musicPlayerContainer = new Container()
                {
                    RelativeSizeAxes = Axes.Both
                });

                pageContainer.Add(bgmListToolboxGroup = new KCSToolboxGroup()
                {
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.None,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Position = new Vector2(-15, 15),
                    Size = new Vector2(300, 0),
                    AutoSizeAxes = Axes.Y,
                    GroupText = "BGM List",
                    Masking = true,
                    CornerRadius = 12.5f,
                    BackgroundColour = Colour4.FromHex("2A2A2A9F"),
                    Child = bgmListBox = new KCSListBox()
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 300,
                        BackgroundColour = Colour4.FromHex("FFFFFF3A"),
                        CornerRadius = 5,
                    }
                });

                screen.tabControlContainer.Add(pageContainer);
                KartStorageFolder? bgmFolder = storageSystem.GetFolder("sound_/bgm");
                KartStorageFile? bgmLocaleFile = storageSystem.GetFile("etc_/bgmList.xml");
                if (bgmFolder is not null && bgmLocaleFile is not null)
                {
                    Queue<KartStorageFolder> bfsQueue = new Queue<KartStorageFolder>();
                    bfsQueue.Enqueue(bgmFolder);
                    while (bfsQueue.Count > 0)
                    {
                        KartStorageFolder dequeuedEle = bfsQueue.Dequeue();
                        foreach (KartStorageFile file in dequeuedEle.Files)
                        {
                            if (file.FullName.EndsWith(".ogg"))
                                if (!trackPaths.ContainsKey(file.NameWithoutExt))
                                    trackPaths.Add(file.NameWithoutExt, file.FullName);
                        }

                        foreach (KartStorageFolder subfolder in dequeuedEle.Folders)
                            bfsQueue.Enqueue(subfolder);
                    }

                    using Stream bgmLocaleXmlDataStream = bgmLocaleFile.CreateStream();

                    XElement bgmStringBagXml = XElement.Load(bgmLocaleXmlDataStream);
                    trackStringBag.LoadFromXElement(bgmStringBagXml);
                    trackStringBag.SetString(CountryCode.TW, "maple_05", "夢之都拉契爾恩");
                    trackStringBag.SetString(CountryCode.TW, "maple_06", "夢境碎片");
                    trackStringBag.SetString(CountryCode.TW, "mu_garage_sunmr", "Raycity 車庫背景音樂");

                    updateBgmList();
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {


        }

        private void onListBoxItemDoubleClicked(ListBoxItem sender)
        {

        }

        private string getDisplayName(string trackName)
        {
            return activeCountryCode switch
            {
                CountryCode.KR => trackStringBag.GetString(CountryCode.KR, trackName),
                CountryCode.CN => trackStringBag.GetString(CountryCode.CN, trackName, false) ??
                                  trackStringBag.GetString(CountryCode.TW, trackName, false) ??
                                  trackStringBag.GetString(CountryCode.KR, trackName),
                CountryCode.TW => trackStringBag.GetString(CountryCode.TW, trackName, false) ??
                                  trackStringBag.GetString(CountryCode.CN, trackName, false) ??
                                  trackStringBag.GetString(CountryCode.KR, trackName),
                _ => trackStringBag.GetString(CountryCode.None, trackName)
            };
        }

        private void updateBgmList()
        {
            bgmListBox.Items.Clear();
            musicPlayerContainer.Clear();
            foreach (var trackName in trackStringBag.Keys)
            {
                string displayName = getDisplayName(trackName);
                bgmListBox.Items.Add(new ListBoxItem(
                    displayName,
                    doubleClickAction: onListBoxItemDoubleClicked,
                    tag: trackName)
                );
            }
        }
    }
}
