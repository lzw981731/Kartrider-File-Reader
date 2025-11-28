using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using KartCityStudio.Game.IO.Stores;
using KartLibrary.Consts;
using KartLibrary.Game.Engine.Track;
using KartLibrary.File;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osuTK;
using osuTK.Audio.OpenAL;
using Vector2 = osuTK.Vector2;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSMusicPlayer : Container
    {
        private readonly Vector2 songTitlePosition = new(55, 35);
        private readonly Vector2 songFileNamePosition = new(55, 88);
        private readonly Box background;
        private readonly Container additionalBackground;
        private readonly SpriteText songTitle;
        private readonly SpriteText songFileName;
        private readonly SpriteText trackTime;
        private readonly KCSIconButton playButton;
        private readonly KCSIconButton repeatButton;
        private readonly KCSIconButton unknownButton;
        private readonly Container amplitudeBoxes;

        private readonly string trackName;
        private readonly string trackPath;

        private KCSSliderBar<double> trackProgressBar;
        private DrawableTrack drawableTrack;

        // Min Setting: 91, Medium Setting: 137, Max Setting: 151
        //
        private int amplitudeBoxesCount = 91;

        private Track track;

        private double prevBoxesUpdateTimestamp = 0;

        private bool unknwonMode = false;

        private Stream baseAudioStream;

        public Container BackgroundExtendContainer => additionalBackground;

        public ColourInfo? BackgroundColour { get; set; }

        public KCSMusicPlayer(Stream audioStream, string trackPath, string trackDisplayName)
        {
            this.trackName = trackDisplayName;
            this.trackPath = trackPath;
            this.baseAudioStream = audioStream;

            Children = new Drawable[]
            {
                additionalBackground = new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                background = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Position = new Vector2(.5f, .5f)
                },
                amplitudeBoxes = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new osuTK.Vector2(0.95f, 1),
                    Alpha = 0,
                    // Scale = new Vector2(0.1f, 1.15f),
                    Scale = new Vector2(0.18f, 0.32f),
                    AlwaysPresent = true,
                    ChildrenEnumerable = Enumerable
                                                .Range(0, amplitudeBoxesCount)
                                                .Select(x =>
                                                new Container()
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.Centre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    RelativePositionAxes = Axes.Both,
                                                    X = (float)x / (amplitudeBoxesCount),
                                                    Width = 0.5f / (amplitudeBoxesCount),
                                                    Height = 0f,
                                                    Masking = true,
                                                    CornerRadius = 3f,
                                                    Child = new Box()
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = Colour4.FromHex("FFFFFF7A"),
                                                        Width = 1f,
                                                        Height = 1f,
                                                    }
                                                })
                },
                songTitle = new SpriteText()
                {
                    RelativePositionAxes = Axes.None,
                    Position = songTitlePosition,
                    Text = trackDisplayName,
                    Font = KCSFont.DefaultL,
                    Colour = Colour4.FromHex("FFFFFFC9"),
                    Alpha = 0f,
                },
                songFileName = new SpriteText()
                {
                    RelativePositionAxes = Axes.None,
                    Position = songFileNamePosition,
                    Text = trackPath,
                    Font = KCSFont.DefaultM,
                    Colour = Colour4.FromHex("FFFFFF7A"),
                    Alpha = 0f
                },
                trackTime = new SpriteText()
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.X,
                    Position = new osuTK.Vector2(0.05f, -40f),
                    Text = "",
                    Font = KCSFont.Default,
                    Colour = Colour4.FromHex("FFFFFF"),
                    Alpha = 0.8f
                },
                trackProgressBar = new KCSSliderBar<double>()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.X,
                    X = 0,
                    Y = -30,
                    Width = 0.9f,
                    Height = 8,
                    MinValue = 0,
                    MaxValue = 100,
                    BackgroundColour = Colour4.FromHex("FFFFFF3A"),
                    ForegroundColour = Colour4.FromHex("FFFFFF"),
                },
                playButton = new KCSIconButton()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.None,
                    X = 0, Y = -75,
                    Width = 65,
                    Height = 65,
                    Icon = FontAwesome.Solid.PauseCircle,
                    BackgroundColour = Colour4.Transparent,
                    HoverColour = Colour4.FromHex("0000007A"),
                    Action = onPlayButtonClicked
                },
                repeatButton = new KCSIconButton()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.None,
                    X = -55, Y = -75,
                    Width = 35,
                    Height = 35,
                    Icon = FontAwesome.Solid.Redo,
                    BackgroundColour = Colour4.Transparent,
                    HoverColour = Colour4.FromHex("0000007A"),
                    Action = onRepeatButtonClicked
                },
                unknownButton = new KCSIconButton()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.None,
                    X = 55, Y = -75,
                    Width = 35,
                    Height = 35,
                    Icon = FontAwesome.Solid.Question,
                    BackgroundColour = Colour4.Transparent,
                    HoverColour = Colour4.FromHex("0000007A"),
                    Action = onUnknownButtonClicked
                }
            };
            background.Colour = new osu.Framework.Graphics.Colour.ColourInfo()
            {
                TopLeft = Colour4.FromHex("000000"),
                TopRight = Colour4.FromHex("0F0F0F"),
                BottomLeft = Colour4.FromHex("1F1F1F"),
                BottomRight = Colour4.FromHex("2F2F2F"),

            };
            trackProgressBar.OnUserChangeValue = seekTrack;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager)
        {
            StreamResourceStore resourceStore = new StreamResourceStore();
            resourceStore.AddStreamSource("base_audio", this.baseAudioStream);
            ITrackStore trackStore = audioManager.GetTrackStore(resourceStore);
            track = trackStore.Get("base_audio");

            drawableTrack = new DrawableTrack(track);
            drawableTrack.Completed += onDrawableTrackCompleted;
            drawableTrack.Start();
            trackProgressBar.MaxValue = track.Length;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            osuTK.Vector2 moveVec = new osuTK.Vector2(0, 75f);
            Tuple<Colour4, Colour4> themeColor = getThemeColor(trackPath);
            background.FadeColour(BackgroundColour ?? new osu.Framework.Graphics.Colour.ColourInfo()
            {
                TopLeft = Colour4.FromHex("000000"),
                TopRight = Colour4.FromHex("0F0F0F"),
                BottomLeft = (themeColor.Item1),
                BottomRight = (themeColor.Item2),
            }, 1533, Easing.OutQuint);

            amplitudeBoxes
                .FadeIn(1233, Easing.OutQuart)
                .ScaleTo(new Vector2(1), 933, Easing.OutQuint);

            // Title original duration: 920 -> 1022
            // File name original duration: 1090 -> 1211
            songTitle
                .MoveToOffset(-moveVec)
                .FadeTo(1.0f, 1022, Easing.OutQuint)
                .MoveToOffset(moveVec, 862, Easing.OutElasticQuarter)
                ;

            songFileName
                .MoveToOffset(-moveVec)
                .FadeTo(1.0f, 1090, Easing.OutQuint)
                .MoveToOffset(moveVec, 997, Easing.OutElasticQuarter)
                ;
        }

        protected override void Update()
        {
            base.Update();
            double boxUpdateDuration = Time.Current - prevBoxesUpdateTimestamp;
            double threshold = 15;
            //float boxWidth = 0.3f + Math.Clamp((DrawSize.X / 350));
            for (int i = 0; i < amplitudeBoxesCount; i++)
            {
                var updateBox = amplitudeBoxes[i];
                if(updateBox is Container container)
                {
                    container.CornerRadius = container.DrawSize.X / 2f;
                }
            }
            if (boxUpdateDuration >= threshold && drawableTrack is not null)
            {
                var freqAmps = drawableTrack.CurrentAmplitudes.FrequencyAmplitudes.Span;
                for(int i = 0; i < amplitudeBoxesCount; i++)
                {
                    var updateBox = amplitudeBoxes[i];
                    // FadeInThreshold should be in [0, 0.9].
                    double fadeInThreshold = Math.Pow(Math.Abs(1 - (i / (float)(amplitudeBoxesCount - 1) * 2)), 1.12) * 0.825;
                    float fadeInMinAlpha = 0.2f;
                    int freqIndex = i * (freqAmps.Length - 1) / (amplitudeBoxesCount - 1);
                    float freqAmpValue = freqAmps[(freqAmps.Length >> 1) - 1 - Math.Min(freqIndex, (freqAmps.Length - 1 - freqIndex))];
                    if (updateBox.Height < freqAmpValue)
                    {
                        updateBox.ClearTransforms();
                        updateBox.Height = freqAmpValue;
                        updateBox.ResizeHeightTo(0, threshold * 30, Easing.None);
                        if(amplitudeBoxes.Alpha >= (fadeInThreshold + 0.1f))
                            updateBox.FadeIn().FadeTo(0.6f, threshold * 30, Easing.OutCubic);
                        // if (i == ((amplitudeBoxesCount >> 1) - 1))
                        // {
                        //     background.ClearTransforms(targetMember: "Height");
                        //     background.Height = (1 + freqAmpValue * 0.15f);
                        //     background.ResizeHeightTo(1, threshold * 30, Easing.None);
                        // }
                    }

                    if (amplitudeBoxes.Alpha < (fadeInThreshold + 0.1f))
                    {
                        if (amplitudeBoxes.Alpha >= fadeInThreshold)
                            updateBox.Alpha = fadeInMinAlpha + ((float)Math.Clamp((amplitudeBoxes.Alpha - fadeInThreshold) * 10f, 0, 1)) * (1 - fadeInMinAlpha);
                        else
                            updateBox.Alpha = fadeInMinAlpha;
                    }
                }
                trackProgressBar.Value = track.CurrentTime;
                TimeSpan trackCurrentTime = TimeSpan.FromMilliseconds(track.CurrentTime);
                TimeSpan trackLength = TimeSpan.FromMilliseconds(track.Length);
                trackTime.Text = $"{trackCurrentTime:mm\\:ss} / {trackLength:mm\\:ss}";
                if (unknwonMode)
                {
                    double val = Math.Pow((drawableTrack.CurrentTime + 320) / drawableTrack.Length, 2f) * 4;
                    drawableTrack.Frequency.Value = Math.Clamp(0.5 + val, 0.3, 4.5);
                    amplitudeBoxes.ScaleTo((float)(drawableTrack.Frequency.Value - 0.5f) * 5 + 0.5f, 320, Easing.OutQuint);
                    unknownButton.RotateTo((float)(drawableTrack.Frequency.Value - 0.5) * 1080, 320, Easing.OutQuint);
                    if (drawableTrack.Frequency.Value >= 1.5f)
                    {
                        // background.RotateTo((float)(drawableTrack.Frequency.Value - 1.75) * 360, 320, Easing.OutQuint);
                        // background.ScaleTo((float)((drawableTrack.Frequency.Value - 1) * 5f) + 1, 320, Easing.OutQuint);
                        double sinVal = Math.Sin(((drawableTrack.Frequency.Value - 1.5) / 3) * Math.PI)  * 3;
                        songTitle
                            .RotateTo((float)((drawableTrack.Frequency.Value - 1.5) * 2880), 320, Easing.OutQuint)
                            .ScaleTo((float)(sinVal * 7f) + 1, 320, Easing.OutQuint)
                            .MoveTo(
                                songTitlePosition + Vector2.Multiply(new Vector2(1f, MathF.Sqrt(2)), (float)(sinVal * 300)),
                                320,
                                Easing.OutQuint
                            );
                        songFileName
                            .RotateTo((float)((drawableTrack.Frequency.Value - 1.5) * -3600), 320, Easing.OutQuint)
                            .ScaleTo((float)(sinVal * 7.5f) + 1, 320, Easing.OutQuint)
                            .MoveTo(
                            songFileNamePosition + Vector2.Multiply(new Vector2(MathF.Sqrt(2), 1f), (float)(sinVal * 300f)),
                            320,
                            Easing.OutQuint
                        );
                    }
                    else
                    {

                        if (drawableTrack.Frequency.Value < 1.5 && Math.Abs(songTitle.Rotation - 8280) < 360 && Math.Abs(songFileName.Rotation + 10440) < 360)
                        {
                            songTitle.RotateTo(songTitle.Rotation  - 8640, 0, Easing.None);
                            songFileName.RotateTo(songFileName.Rotation  + 10800, 0, Easing.None);
                        }
                        songTitle
                            .RotateTo(0, 320, Easing.OutQuint)
                            .ScaleTo(1, 320, Easing.OutQuint)
                            .MoveTo(songTitlePosition, 320, Easing.OutQuint);
                        songFileName
                            .RotateTo(0, 320, Easing.OutQuint)
                            .ScaleTo(1, 320, Easing.OutQuint)
                            .MoveTo(songFileNamePosition, 320, Easing.OutQuint);
                    }
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            track.Stop();
            track.Dispose();
            base.Dispose(isDisposing);
        }

        private void seekTrack(double time)
        {
            time = Math.Clamp(time, 0, track.Length - 0.3f);
            track.Seek(time);
        }

        private void onPlayButtonClicked()
        {
            if (track is not null)
                if (drawableTrack.IsRunning)
                {
                    drawableTrack.Stop();
                    playButton.Icon = FontAwesome.Solid.PlayCircle;
                }
                else
                {
                    if (drawableTrack.CurrentTime >= drawableTrack.Length)
                        drawableTrack.Seek(0);
                    drawableTrack.Start();
                    playButton.Icon = FontAwesome.Solid.PauseCircle;
                }
        }

        private void onRepeatButtonClicked()
        {
            if(track is not null)
                if(drawableTrack.Looping)
                {
                    repeatButton.BackgroundColour = Colour4.Transparent;
                    drawableTrack.Looping = false;
                }
                else
                {
                    repeatButton.BackgroundColour = Colour4.FromHex("ECECEC6A");
                    drawableTrack.Looping = true;
                    if (drawableTrack.CurrentTime >= drawableTrack.Length)
                        playButton.Icon = FontAwesome.Solid.PauseCircle;
                }
        }

        private void onUnknownButtonClicked()
        {
            if (track is not null)
                if (unknwonMode)
                {
                    drawableTrack.Frequency.Value = 1;
                    unknownButton.BackgroundColour = Colour4.Transparent;
                    unknownButton.RotateTo(0, 720, Easing.OutElasticQuarter);
                    amplitudeBoxes.ScaleTo(1, 720, Easing.OutElasticQuarter);
                    songTitle
                        .RotateTo(0, 320, Easing.OutQuint)
                        .ScaleTo(1, 320, Easing.OutQuint)
                        .MoveTo(songTitlePosition, 720, Easing.OutElasticQuarter);
                    songFileName
                        .RotateTo(0, 320, Easing.OutQuint)
                        .ScaleTo(1, 320, Easing.OutQuint)
                        .MoveTo(songFileNamePosition, 720, Easing.OutElasticQuarter);
                    unknwonMode = false;
                }
                else
                {
                    unknownButton.BackgroundColour = Colour4.FromHex("637E765F");
                    unknwonMode = true;
                }
        }

        private void onDrawableTrackCompleted()
        {
            playButton.Icon = FontAwesome.Solid.PlayCircle;
            drawableTrack.Stop();
        }
        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (drawableTrack.IsRunning && !unknwonMode)
            {
                if(e.Key == osuTK.Input.Key.P)
                {
                    drawableTrack.Frequency.Value = Math.Clamp(drawableTrack.Frequency.Value * 1.005, 1, 2.86);
                }
                if(e.Key == osuTK.Input.Key.O)
                {
                    drawableTrack.Frequency.Value = Math.Clamp(drawableTrack.Frequency.Value * 0.995, 0.35, 1);
                }
                if (e.Key == osuTK.Input.Key.I)
                {
                    float relMouseX = ((e.MousePosition.X / DrawSize.X) * 2) - 1;
                    float relMouseY = ((e.MousePosition.Y / DrawSize.Y) * 2) - 1;
                    double val = (relMouseX + relMouseY) * 1.86;
                    double scale = val < 0 ? (1 / (1 - val)) : (1 + val);
                    drawableTrack.Frequency.Value = Math.Clamp(scale, 0.35, 2.86);

                }
                songTitle.ScaleTo(new osuTK.Vector2((float)drawableTrack.Frequency.Value, songTitle.Scale.Y), 320, Easing.OutQuint);
            }
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == osuTK.Input.Key.P || e.Key == osuTK.Input.Key.O || e.Key == osuTK.Input.Key.I)
                if (drawableTrack.IsRunning)
                {
                    drawableTrack.Frequency.Value = 1;
                    songTitle.ScaleTo(1, 720, Easing.OutElasticQuarter);
                }
            base.OnKeyUp(e);
        }

        private Tuple<Colour4, Colour4> getThemeColor(string trackPath)
        {
            Dictionary<string, Tuple<Colour4, Colour4>> themes = new Dictionary<string, Tuple<Colour4, Colour4>>()
            {
                ["china"] = new(Colour4.FromHex("7D0A0A"), Colour4.FromHex("BF3131")),
                ["desert"] = new(Colour4.FromHex("6F4E37"), Colour4.FromHex("A67B5B")),
                ["factory"] = new(Colour4.FromHex("405D72"), Colour4.FromHex("758694")),
                ["other"] = new(Colour4.FromHex("1F1F1F"), Colour4.FromHex("2F2F2F")),
                ["ice"] = new(Colour4.FromHex("6EACDA"), Colour4.FromHex("3ABEF9")),
                ["northeu"] = new(Colour4.FromHex("180161"), Colour4.FromHex("4F1787")),
                ["moonhill"] = new(Colour4.FromHex("071952"), Colour4.FromHex("03346E")),
                ["tomb"] = new(Colour4.FromHex("373A40"), Colour4.FromHex("686D76")),
                ["forest"] = new(Colour4.FromHex("365E32"), Colour4.FromHex("81A263")),
                ["mine"] = new(Colour4.FromHex("2E236C"), Colour4.FromHex("433D8B")),
                ["korea"] = new(Colour4.FromHex("8e7847"), Colour4.FromHex("bfa573")),
                ["abyss"] = new(Colour4.FromHex("1A3636"), Colour4.FromHex("40534C")),
                ["olympos"] = new(Colour4.FromHex("088395"), Colour4.FromHex("37B7C3")),
                ["boss"] = new (Colour4.FromHex("543310"), Colour4.FromHex("FFC700")),
                ["maple"] = new (Colour4.FromHex("EB5B00"), Colour4.FromHex("FFB200")),
                ["maple_05"] = new (Colour4.FromHex("180161"), Colour4.FromHex("9F0D7F")),
                ["maple_06"] = new (Colour4.FromHex("180161"), Colour4.FromHex("9F0D7F")),
                ["transFormer"] = new (Colour4.FromHex("072541"), Colour4.FromHex("304463")),
                ["world"] = new (Colour4.FromHex("40A2E3"), Colour4.FromHex("78C1F3")),
                ["jurassic"] = new (Colour4.FromHex("377D71"), Colour4.FromHex("97C4B8")),
                ["fairy"] = new (Colour4.FromHex("7469B6"), Colour4.FromHex("AD88C6")),
                ["xmas"] = new (Colour4.FromHex("AA2B1D"), Colour4.FromHex("EFB08C")),
                ["gold"] = new (Colour4.FromHex("E9B824"), Colour4.FromHex("FFC700")),
                ["nymph"] = new (Colour4.FromHex("4C3BCF"), Colour4.FromHex("4B70F5")),
                ["pirate"] = new (Colour4.FromHex("A79277"), Colour4.FromHex("D1BB9E")),
                ["village"] = new (Colour4.FromHex("AF8260"), Colour4.FromHex("D4BDAC")),
                ["castle"] = new (Colour4.FromHex("DEAC80"), Colour4.FromHex("E0A75E")),
                ["camelot"] = new (Colour4.FromHex("CBA35C"), Colour4.FromHex("A59D84")),
                ["flag"] = new (Colour4.FromHex("03346E"), Colour4.FromHex("134B70")),
                ["title"] = new (Colour4.FromHex("0055ac"), Colour4.FromHex("0055ac")),
                ["nemo"] = new (Colour4.FromHex("1C3879"), Colour4.FromHex("035397")),
                ["sword"] = new (Colour4.FromHex("6F6F6F"), Colour4.FromHex("3F3F3F")),
                ["wkc"] = new (Colour4.FromHex("2A2B2C"), Colour4.FromHex("ACACAC")),
                ["fengshen"] = new (Colour4.FromHex("8D77AB"), Colour4.FromHex("7C93C3")),
            };
            string[] splits = trackPath.Split('/');
            Tuple<Colour4, Colour4> outColor;
            string theme = splits[^1].Replace(".ogg", "");
            if (themes.TryGetValue(theme, out outColor))
                return outColor;

            theme = splits.Length >= 2 ? splits[^2].TrimEnd('2') : "other";;
            if (themes.TryGetValue(theme, out outColor))
                return outColor;

            theme = theme = splits[^1].Split('_')[0].ToLower();
            if (themes.TryGetValue(theme, out outColor))
                return outColor;
            return themes["other"];
        }
    }
}
