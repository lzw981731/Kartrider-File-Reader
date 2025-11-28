using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osuTK;

namespace KartCityStudio.Game.Graphics.UserInterface
{
    public partial class KCSFilePicker: CompositeDrawable
    {
        private readonly Container contentContainer;
        private readonly Box background;
        private readonly KCSSplittableContainer splittableContainer;

        private readonly KCSToolboxGroup volumesGroup;
        private readonly KCSTreeView shortCutTreeView;
        private readonly KCSToolboxGroup systemGroup;
        private readonly KCSListBox systemListBox;

        private readonly Container rightPanelContainer;
        private readonly GridContainer fileListBtnContainer;
        private readonly KCSTextbox currentPathTextbox;
        private readonly KCSTextbox searchingTextbox;
        private readonly KCSButton backBtn;
        private readonly KCSButton nextBtn;
        private readonly KCSButton backToParentBtn;

        private readonly KCSLoadingSpinner leftSideLoadingSpinner;
        private readonly KCSLoadingSpinner rightSideLoadingSpinner;
        private readonly Box rightSideLoadingMask;

        private readonly KCSListView fileListView;

        private DirectoryInfo? currentDirectory;
        private string nextPath;

        private bool initLeftSideWorkerFinished;
        private System.ComponentModel.BackgroundWorker initLeftSideWorker;

        private bool updateRightSideWorkerFinished;
        private System.ComponentModel.BackgroundWorker updateRightSideWorker;

        private Regex? fileNameFilter;
        private FilePickMode filePickMode;

        private readonly KCSButton selectBtn;
        private readonly KCSButton cancelBtn;
        private readonly SpriteText selectBtnText;

        public event Action<FileInfo> FileSelected;

        public event Action<DirectoryInfo> DirectorySelected;

        public event Action Canceled;

        public Regex? FileNameFilter
        {
            get => fileNameFilter;
            set
            {
                fileNameFilter = value;
                updatesFileNameFilter();
            }
        }

        public FilePickMode PickMode
        {
            get => filePickMode;
            set
            {
                filePickMode = value;
                updatesFileNameFilter();
            }
        }

        public ColourInfo BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public KCSFilePicker()
        {
            nextPath = Environment.CurrentDirectory;


            Masking = true;
            CornerRadius = 20f;

            InternalChild = contentContainer = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new ColourInfo()
                        {
                            TopLeft = Colour4.FromHex("2222227F"),
                            TopRight = Colour4.FromHex("3333337F"),
                            BottomLeft = Colour4.FromHex("5555557F"),
                            BottomRight = Colour4.FromHex("7777777F"),
                        }
                    },
                    splittableContainer = new KCSSplittableContainer(Direction.Horizontal)
                    {
                        RelativeSizeAxes = Axes.Both,
                        SplitterBarPosition = 0.25f,
                    },
                }
            };

            splittableContainer.FirstContainer.Masking = false;
            splittableContainer.FirstContainer.Add(new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.FromHex("0000005C")
            });
            splittableContainer.FirstContainer.Add(new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding() { Vertical = 4f, Horizontal = 0f},
                Child = shortCutTreeView = new KCSTreeView()
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = Colour4.Transparent,
                }
            });

            splittableContainer.FirstContainer.Add(
                leftSideLoadingSpinner = new KCSLoadingSpinner()
                {
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(70),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            );
            splittableContainer.SecondContainer.Add(new GridContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding() { Horizontal = 10f, Vertical = 10f },
                RowDimensions = new Dimension[]
                {
                    new Dimension(mode: GridSizeMode.Absolute, size: 40f),
                    new Dimension(mode: GridSizeMode.Distributed),
                    new Dimension(mode: GridSizeMode.Absolute, size: 41f)
                },
                ColumnDimensions = new Dimension[]
                {
                    new Dimension(mode: GridSizeMode.Distributed),
                },
                Content = new Drawable[][]
                {
                    new Drawable[]
                    {
                        fileListBtnContainer = new GridContainer()
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new Dimension[]
                            {
                                new Dimension(mode: GridSizeMode.AutoSize),
                            },
                            ColumnDimensions = new Dimension[]
                            {
                                new Dimension(mode: GridSizeMode.AutoSize),
                                new Dimension(mode: GridSizeMode.Distributed),
                                new Dimension(mode: GridSizeMode.Relative, size: 0.3f, maxSize: 300f),
                            },
                        }
                    },
                    new Drawable[]
                    {
                        fileListView = new KCSListView()
                        {
                            RelativeSizeAxes = Axes.Both,
                            BackgroundColour = Colour4.FromHex("0000007F"),
                            HeaderColour = Colour4.FromHex("0000007F"),
                        }
                    },
                    new Drawable[]
                    {
                        new FillFlowContainer()
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Masking = true,
                            Height = 32f,
                            CornerRadius = 8,
                            Spacing = new Vector2(5f),
                            RelativeSizeAxes = Axes.None,
                            AutoSizeAxes = Axes.X,
                            Children = new KCSButton[]
                            {
                                selectBtn = new KCSButton()
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    BackgroundColour = Colour4.FromHex("0000009A"),
                                    HoverColour = Colour4.FromHex("3A3A3A3A"),
                                    Masking = true,
                                    CornerRadius = 10,
                                    Width = 100,
                                    ScaleWhenButtonDown = false,
                                    Action = onSelectBtnClicked,
                                    Child = selectBtnText = new SpriteText()
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativePositionAxes = Axes.Y,
                                        Text = "Select",
                                        Font = KCSFont.Default,
                                        Colour = Colour4.FromHex("FFFFFF"),
                                        Alpha = 0.8f
                                    }
                                },
                                cancelBtn = new KCSButton()
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    BackgroundColour = Colour4.FromHex("0000005A"),
                                    HoverColour = Colour4.FromHex("FFFFFF3A"),
                                    Masking = true,
                                    CornerRadius = 10,
                                    Width = 100,
                                    ScaleWhenButtonDown = false,
                                    Action = onCancleBtnClicked,
                                    Child = new SpriteText()
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativePositionAxes = Axes.Y,
                                        Text = "Cancel",
                                        Font = KCSFont.Default,
                                        Colour = Colour4.FromHex("FFFFFF"),
                                        Alpha = 0.8f
                                    }
                                }
                            }
                        },
                    }
                }
            });

            fileListBtnContainer.Content = new Drawable[][]
            {
                new Drawable[]
                {
                    new KCSButtonGroup(FillDirection.Horizontal)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.None,
                        Height = 30f,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new KCSButton[]
                        {
                            backBtn = new KCSIconButton()
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Width = 32f,
                                IconRelativeSize = new osuTK.Vector2(0.4f),
                                Icon = FontAwesome.Solid.ChevronLeft,
                                BackgroundColour = Colour4.FromHex("00000000"),
                                HoverColour = Colour4.FromHex("FFFFFF3A"),
                            },
                            nextBtn = new KCSIconButton()
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Width = 32f,
                                IconRelativeSize = new osuTK.Vector2(0.4f),
                                Icon = FontAwesome.Solid.ChevronRight,
                                BackgroundColour = Colour4.FromHex("00000000"),
                                HoverColour = Colour4.FromHex("FFFFFF3A"),
                                Masking = true,
                                CornerRadius = 0,
                            },
                            backToParentBtn = new KCSIconButton()
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Width = 32f,
                                IconRelativeSize = new osuTK.Vector2(0.4f),
                                Icon = FontAwesome.Solid.ChevronUp,
                                BackgroundColour = Colour4.FromHex("00000000"),
                                HoverColour = Colour4.FromHex("FFFFFF3A"),
                                Masking = true,
                                CornerRadius = 0,
                                Action = backToParentFolder
                            }
                        }
                    },
                    currentPathTextbox = new KCSTextbox()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        BackgroundColour = Colour4.FromHex("0000008A"),
                        Padding = new MarginPadding() { Horizontal = 5f },
                        Height = 30f,
                        CornerRadius = 10f,
                    },
                    searchingTextbox = new KCSTextbox()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        BackgroundColour = Colour4.FromHex("0000008A"),
                        Height = 30f,
                        CornerRadius = 10f,
                    }
                }
            };


            splittableContainer.SecondContainer.Add(
                rightSideLoadingSpinner = new KCSLoadingSpinner()
                {
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(70),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            );

            splittableContainer.SecondContainer.Add(
                rightSideLoadingMask = new Box()
                {
                    AlwaysPresent = false,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.FromHex("0000009A")
                }
            );

            fileListView.Headers.Add(new ListViewHeaderItem("Name", "Name", 0.50f));
            fileListView.Headers.Add(new ListViewHeaderItem("DateModified", "Date modified", 0.20f));
            fileListView.Headers.Add(new ListViewHeaderItem("Type", "Type", 0.15f));
            fileListView.Headers.Add(new ListViewHeaderItem("Size", "Size", 0.15f));
            fileListView.ItemDoubleClicked += fileListViewItemDoubleClicked;
            fileListView.SelectdItemChanged += fileListViewItemSelectedItemChanged;
        }

        private void onCancleBtnClicked()
        {
            Canceled ?.Invoke();
        }


        [BackgroundDependencyLoader]
        private void load()
        {
            initLeftSideWorkerFinished = true;
            initLeftSideWorker = new System.ComponentModel.BackgroundWorker();
            initLeftSideWorker.WorkerSupportsCancellation = true;
            initLeftSideWorker.DoWork += initLeftSideWork;
            initLeftSideWorker.RunWorkerCompleted += initLeftSideWorkFinished;

            startBgInitLeftSideWork();

            updateRightSideWorkerFinished = true;
            updateRightSideWorker = new System.ComponentModel.BackgroundWorker();
            updateRightSideWorker.DoWork += updateRightSideWork;
            updateRightSideWorker.RunWorkerCompleted += updateRightSideWorkFinished;

            startBgUpdateRightSideWork();
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds)
        {
            return false;
        }

        private void startBgInitLeftSideWork(int recursionCount = 0)
        {
            if (initLeftSideWorkerFinished)
            {
                initLeftSideWorkerFinished = false;
                initLeftSideWorker.RunWorkerAsync();
            }
            else
            {
                Logger.Log($"{nameof(startBgInitLeftSideWork)} still waiting previous works end. Waiting frames: {recursionCount}");
                Scheduler.AddOnce(startBgInitLeftSideWork, recursionCount + 1);
            }
        }

        private void initLeftSideWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Scheduler.AddOnce(leftSideLoadingSpinner.Show);
            if (initLeftSideWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                if (initLeftSideWorker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Result = new Tuple<DriveInfo[]>(drives);
                }
            }

        }

        private void initLeftSideWorkFinished(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Scheduler.AddOnce(leftSideLoadingSpinner.Hide);
                initLeftSideWorkerFinished = true;
            }
            else if (e.Error is not null)
            {
                Scheduler.AddOnce(leftSideLoadingSpinner.Hide);
                initLeftSideWorkerFinished = true;
            }
            else
            {
                if(e.Result is Tuple <DriveInfo[]> result)
                {
                    Scheduler.AddOnce(initLeftSide, result);
                }
            }
        }

        private void initLeftSide(Tuple<DriveInfo[]> result)
        {
            shortCutTreeView.Nodes.Clear();
            TreeViewNode volumeNode = new TreeViewNode("Volumes")
            {
                Expanded = { Value = true },
                HideExpandIcon = { Value = true },
                Selectable = { Value = false }
            };
            TreeViewNode systemNode = new TreeViewNode("System")
            {
                Expanded = { Value = true },
                HideExpandIcon = { Value = true },
                Selectable = { Value = false }
            };
            foreach(var drive in result.Item1)
            {
                string driveStr = $"";
                if(drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.CDRom)
                    volumeNode.Nodes.Add(
                        new TreeViewNode(
                            $"{(drive.VolumeLabel == "" ? "Local Disk" : drive.VolumeLabel)}({drive.RootDirectory})",
                            iconName: "ic_fluent_storage_24_filled",
                            tag: drive,
                            clickAction: onShortcutNodeClicked
                        )
                        {
                            HideExpandIcon    = { Value = true }
                        }
                    );
            }
            shortCutTreeView.Nodes.Add(volumeNode);
            shortCutTreeView.Nodes.Add(systemNode);
            leftSideLoadingSpinner.Hide();

            initLeftSideWorkerFinished = true;
        }

        private void startBgUpdateRightSideWork(int recursionCount = 0)
        {
            if (updateRightSideWorkerFinished)
            {
                updateRightSideWorkerFinished = false;
                updateRightSideWorker.RunWorkerAsync();
            }
            else
            {
                // This work will be canceled if it was waiting to executing for 100 frames.
                if (recursionCount < 100)
                {
                    Logger.Log($"{nameof(startBgUpdateRightSideWork)} still waiting previous works end. Waiting frames: {recursionCount}");
                    Scheduler.AddOnce(startBgUpdateRightSideWork, recursionCount + 1);
                }
            }
        }

        private void updateRightSideWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Scheduler.AddOnce(rightSideLoadingSpinner.Show);
            Scheduler.AddOnce(() => rightSideLoadingMask.FadeIn(340));
            string localNextPath = nextPath;  // nextPath may update by other thread
            DirectoryInfo localNextDirectoryInfo = new DirectoryInfo(localNextPath);
            if (!localNextDirectoryInfo.Exists)
            {
                lock (nextPath)
                {
                    if(nextPath == localNextPath)
                        nextPath = currentDirectory.FullName;
                }
                throw new DirectoryNotFoundException(nextPath);
            }
            else if(currentDirectory?.FullName != nextPath)
            {
                e.Result = new Tuple<ArraySegment<DirectoryInfo>, ArraySegment<FileInfo>>(
                    localNextDirectoryInfo.GetDirectories(),
                    localNextDirectoryInfo.GetFiles());
                currentDirectory = localNextDirectoryInfo;
            }
        }

        private void updateRightSideWorkFinished(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Scheduler.AddOnce(rightSideLoadingSpinner.Hide);
                Scheduler.AddOnce(() => rightSideLoadingMask.FadeOut(340));
            }
            else if(e.Error is not null)
            {
                Logger.Log($"{e.Error}");
                Scheduler.AddOnce(() =>
                {
                    currentPathTextbox.ClearTransforms();
                    currentPathTextbox
                        .FadeColour(Colour4.FromHex("FF0000"), 139)
                        .Then()
                        .FadeColour(Colour4.FromHex("FFFFFF"), 539, Easing.OutQuint);
                });
                Scheduler.AddOnce(rightSideLoadingSpinner.Hide);
                Scheduler.AddOnce(() => rightSideLoadingMask.FadeOut(340));
            }
            else
            {
                if(e.Result is Tuple<ArraySegment<DirectoryInfo>, ArraySegment<FileInfo>> result)
                {
                    Scheduler.Add(updatesDirectoryList, result);
                }
                else
                {
                    Scheduler.AddOnce(rightSideLoadingSpinner.Hide);
                    Scheduler.AddOnce(() => rightSideLoadingMask.FadeOut(340));
                }
            }
            updateRightSideWorkerFinished = true;
        }

        private void updatesDirectoryList(Tuple<ArraySegment<DirectoryInfo>, ArraySegment<FileInfo>> result)
        {
            fileListView.LayoutEasing = Easing.None;
            fileListView.LayoutDuration = 0;
            fileListView.Items.Clear();
            IEnumerable<ListViewItem> dirItems = result.Item1.Select(x => new ListViewItem(
                new (string key, osu.Framework.Localisation.LocalisableString value)[]
                {
                    ("Name", x.Name),
                    ("DateModified", x.LastWriteTime.ToString()),
                    ("Type", "Folder"),
                    ("Size", ""),
                },
                iconName: "folder_close"
            )
            {
                Tag = { Value = x }
            });
            IEnumerable<ListViewItem> fileItems = result.Item2.Select(x => new ListViewItem(
                new (string key, osu.Framework.Localisation.LocalisableString value)[]
                {
                    ("Name", x.Name),
                    ("DateModified", x.LastWriteTime.ToString()),
                    ("Type", "File"),
                    ("Size", Utilities.UnitUtility.FormatDataSize(x.Length)),
                },
                iconName: "ic_fluent_document_24_filled"
            )
            {
                Tag = { Value = x },
                Visible = { Value = FileNameFilter?.IsMatch(x.Name) ?? true }
            });
            applyResultToFileListView((dirItems.Concat(fileItems)).ToArray());
        }

        private void applyResultToFileListView(ArraySegment<ListViewItem> items)
        {
            const double max_waiting_time = 3;
            const int segment_size = 300;

            int endIndex = (Math.Min(items.Count, segment_size));
            ArraySegment<ListViewItem> itemsToApply = items[..endIndex];

            DateTime beginTime = DateTime.Now;
            TimeSpan duration = DateTime.Now - beginTime;

            while (itemsToApply.Count > 0 && duration.TotalMilliseconds < (max_waiting_time / 2f))
            {
                fileListView.Items.AddRange(itemsToApply);
                itemsToApply = items[endIndex..];
                endIndex = (Math.Min(itemsToApply.Count, segment_size));
                duration = DateTime.Now - beginTime;
            }

            if (itemsToApply.Count > 0)
            {
                Scheduler.AddOnce(applyResultToFileListView, itemsToApply[endIndex..]);
            }
            else
            {
                currentPathTextbox.Text = currentDirectory?.FullName ?? "";
                rightSideLoadingSpinner.Hide();
                Scheduler.AddOnce(() => rightSideLoadingMask.FadeOut(340));
                updateRightSideWorkerFinished = true;
            }
        }

        private void fileListViewItemDoubleClicked(ListViewItem obj)
        {
            if (obj.Tag.Value is DirectoryInfo directoryInfo)
            {
                nextPath = directoryInfo.FullName;
                startBgUpdateRightSideWork();
            }
            else if (obj.Tag.Value is FileInfo fileInfo)
            {
                FileSelected?.Invoke(fileInfo);
            }
        }

        private void fileListViewItemSelectedItemChanged()
        {
            selectBtn.Enabled.Value = true;
            if (fileListView.SelectedItem?.Tag.Value is FileInfo && filePickMode.HasFlag(FilePickMode.AllowFile))
                selectBtnText.Text = "Select File";
            else if (fileListView.SelectedItem?.Tag.Value is DirectoryInfo or FileInfo && filePickMode.HasFlag(FilePickMode.AllowDirectory))
                selectBtnText.Text = "Select Folder";
            else
            {
                selectBtnText.Text = "Select";
                selectBtn.Enabled.Value = false;
            }
        }

        private void updatesFileNameFilter()
        {
            fileListView.LayoutEasing = Easing.OutExpo;
            fileListView.LayoutDuration = 750;
            foreach (ListViewItem listViewItem in fileListView.Items)
            {
                if (listViewItem.Tag.Value is FileInfo fileInfo)
                {
                    if (fileNameFilter is not null)
                    {
                        listViewItem.Visible.Value = fileNameFilter.IsMatch(fileInfo.Name);
                    }
                    else
                    {
                        listViewItem.Visible.Value = (PickMode | FilePickMode.AllowFile) == FilePickMode.AllowFile;
                    }
                }
            }
        }

        private void onShortcutNodeClicked(TreeViewNode node)
        {
            if (node.Tag is DriveInfo driveInfo)
            {
                lock (nextPath)
                {
                    DirectoryInfo directoryInfo = driveInfo.RootDirectory;
                    nextPath = directoryInfo.FullName;
                    startBgUpdateRightSideWork();
                }
            }
        }

        private void onSelectBtnClicked()
        {
            if (fileListView.SelectedItem is not null)
            {
                if (filePickMode.HasFlag(FilePickMode.AllowDirectory) && currentDirectory is not null)
                {
                    if(fileListView.SelectedItem.Tag.Value is DirectoryInfo directoryInfo)
                        DirectorySelected?.Invoke(directoryInfo);
                    else if(fileListView.SelectedItem.Tag.Value is FileInfo fileInfo)
                        DirectorySelected?.Invoke(currentDirectory);
                }
                else if (fileListView.SelectedItem.Tag.Value is FileInfo fileInfo && filePickMode.HasFlag(FilePickMode.AllowFile))
                {
                    FileSelected?.Invoke(fileInfo);
                }
            }
            else if (currentDirectory is not null && filePickMode.HasFlag(FilePickMode.AllowDirectory))
            {
                DirectorySelected?.Invoke(currentDirectory);
            }
        }

        private void backToParentFolder()
        {
            lock (nextPath)
            {
                DirectoryInfo? directoryInfo = Directory.GetParent(nextPath);
                if (directoryInfo is not null)
                {
                    nextPath = directoryInfo.FullName;
                    startBgUpdateRightSideWork();
                }
            }
        }
    }

    public enum FilePickMode
    {
        None,
        AllowFile = 1,
        AllowDirectory = 2,
        AllowFileAndDirectory = AllowFile | AllowDirectory,
    }
}
