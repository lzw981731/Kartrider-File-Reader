using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.FileType;
using KartCity.Common.Xml;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.UserInterface;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.Game.Localization;
using KartLibrary.IO;
using KartLibrary.Xml;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osuTK;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneMusicPlayer : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        private KCSMusicPlayer musicPlayer;

        private Container musicPlayerContainer;

        private KCSToolboxGroup bgmListToolboxGroup;

        private KCSListBox bgmListBox;

        private StringBag trackStringBag;

        private Dictionary<string, string> trackPaths;

        private CountryCode activeCountryCode;

        private DialogContainer dialogContainer;

        private KCSFilePicker filePicker;

        public TestSceneMusicPlayer()
        {
            activeCountryCode = CountryCode.KR;
            trackStringBag = new StringBag();
            trackPaths = new Dictionary<string, string>();

            Add(musicPlayerContainer = new Container()
            {
                RelativeSizeAxes = Axes.Both
            });

            Add(bgmListToolboxGroup = new KCSToolboxGroup()
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
                Children = new Drawable[]
                {
                    bgmListBox = new KCSListBox()
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 300,
                        BackgroundColour = Colour4.FromHex("0000003A"),
                        CornerRadius = 5,
                    }
                }
            });

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

            AddStep("Open custom music", () =>
            {
                filePicker.FileNameFilter = new Regex(@"^(.*)\.(?:ogg|mp3|flac|m4a)$");
                filePicker.FileSelected += onCustomMusicFileSelected;
                dialogContainer.Show();
            });
        }

        private void onCustomMusicFileSelected(FileInfo obj)
        {
            playCustomMusic(obj.FullName);
            filePicker.FileSelected -= onCustomMusicFileSelected;
            dialogContainer.Hide();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            KartStorageFolder? bgmFolder = storageSystem.GetFolder("sound/bgm") ?? storageSystem.GetFolder("sound_/bgm");
            KartStorageFile? bgmLocaleFile = storageSystem.GetFile("etc_/bgmList.xml")
                                             ?? storageSystem.GetFile("etc_/bgmList.bml")
                                             ?? storageSystem.GetFile("etc/bgmList.xml")
                                             ?? storageSystem.GetFile("etc/bgmList.bml");
            if (bgmFolder is not null)
            {
                Queue<KartStorageFolder> bfsQueue = new Queue<KartStorageFolder>();
                bfsQueue.Enqueue(bgmFolder);
                while (bfsQueue.Count > 0)
                {
                    KartStorageFolder dequeuedEle = bfsQueue.Dequeue();
                    foreach (KartStorageFile file in dequeuedEle.Files)
                    {
                        if(file.FullName.EndsWith(".ogg"))
                            if(!trackPaths.ContainsKey(file.NameWithoutExt))
                                trackPaths.Add(file.NameWithoutExt, file.FullName);
                    }

                    foreach (KartStorageFolder subfolder in dequeuedEle.Folders)
                        bfsQueue.Enqueue(subfolder);
                }

                if (bgmLocaleFile is not null)
                {
                    using Stream bgmLocaleXmlDataStream = bgmLocaleFile.CreateStream();
                    if (bgmLocaleFile.Name.EndsWith(".xml"))
                    {
                        XElement bgmStringBagXml = XElement.Load(bgmLocaleXmlDataStream);
                        trackStringBag.LoadFromXElement(bgmStringBagXml);
                    }
                    else if(bgmLocaleFile.Name.EndsWith(".bml"))
                    {
                        BinaryReader bmlReader = new BinaryReader(bgmLocaleXmlDataStream);
                        BinaryXmlTag binaryXmlTag = bmlReader.ReadBinaryXmlTag(Encoding.Unicode);
                        trackStringBag.LoadFromBinaryXmlTag(binaryXmlTag);
                    }

                    trackStringBag.SetString(CountryCode.TW, "maple_05", "夢之都拉契爾恩");
                    trackStringBag.SetString(CountryCode.TW, "maple_06", "夢境碎片");
                    trackStringBag.SetString(CountryCode.TW, "mu_garage_sunmr", "Raycity 車庫背景音樂");
                }

                updateBgmList();
            }
        }

        private void onListBoxItemDoubleClicked(ListBoxItem sender)
        {
            if (sender.Tag is string trackName && trackPaths.ContainsKey(trackName))
            {
                string displayName = getDisplayName(trackName);
                string trachPath = trackPaths[trackName];
                musicPlayerContainer.Clear();
                IRhoFile trackFile = storageSystem.GetFile(trachPath);
                if (trackFile is not null)
                {
                    musicPlayerContainer.Child = musicPlayer = new KCSMusicPlayer(trackFile.CreateStream(), trackFile.FullName, displayName)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new osuTK.Vector2(1, 1),
                    };
                }

            }
        }

        private void playCustomMusic(string path)
        {
            FileInfo fi = new FileInfo(path);
            using FileStream fs = new FileStream(fi.FullName, FileMode.Open);
            byte[] data = fs.ReadAllBytesToArray();
            musicPlayerContainer.Child = musicPlayer = new KCSMusicPlayer(new MemoryStream(data), fi.FullName, fi.Name)
            {
                RelativeSizeAxes = Axes.Both,
                Size = new osuTK.Vector2(1, 1),
            };
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
            IEnumerable<string> trackNames = trackStringBag.Keys.Concat(trackPaths.Keys.Except(trackStringBag.Keys));
            foreach (var trackName in trackNames)
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
