using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using KartCityStudio.Common.Model.Archive;
using KartCityStudio.Common.Model.Archive.Implements.KartStorage;
using KartCityStudio.Common.Model.Archive.Implements.Rho;
using KartCityStudio.Common.Service;
using KartCityStudio.Game.Graphics.Containers;
using KartCityStudio.Game.Graphics.Sprites;
using KartCityStudio.Game.Graphics.UserInterface;
using KartCityStudio.Game.Model;
using KartCityStudio.Game.Service;
using KartCityStudio.Model.Archive.Implements.RCStorage;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.Game.Localization;
using KartLibrary.IO;
using KartLibrary.Xml;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;
using RayCityLibrary.File;

namespace KartCityStudio.Game.Screens
{
    public partial class MainScreen : Screen
    {
        private const float MenuHeight = 27;

        public KCSMenu mainMenubar;

        public Container screenContentContainer;

        public DialogContainer dialogContainer;

        private KCSFilePicker filePicker;

        public KCSSplittableContainer splittableContainer;

        public KCSTreeView fileBrowserTreeView;

        public KCSTabControlContainer tabControlContainer;

        public KCSLoadingSpinner loadingSpinner;

        private KCSListView folderView;

        #region UIModel

        private IArchiveModel? archiveModel;

        private IArchiveFolder? currentFolder;

        private GridContainer fileListBtnContainer;

        private KCSIconButton backBtn;

        private KCSIconButton nextBtn;

        private KCSIconButton backToParentBtn;

        private KCSTextbox currentPathTextbox;

        private KCSTextbox searchingTextbox;

        #endregion

        #region UI Initialization
        // This region contains all methods related of UI element constructing.
        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager)
        {
            InternalChildren = new Drawable[]
            {
                screenContentContainer = new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Padding = new MarginPadding()
                    {
                        Top = MenuHeight
                    },
                    Children = new Drawable[]
                    {
                        splittableContainer = new KCSSplittableContainer(Direction.Horizontal)
                        {
                            RelativeSizeAxes = Axes.Both,
                            SplitterBarPosition = 0.3f
                        }
                    }
                },
                new Container() // Menu bar container.
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = MenuHeight,
                    Child = mainMenubar = new KCSMenu()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Items = createMenuItems()
                    }
                },
                dialogContainer = new DialogContainer()
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
                            FileNameFilter = new Regex(@"^(.*)\.(?:rho|jmd)"),
                            BackgroundColour = new ColourInfo()
                            {
                                TopLeft = Colour4.FromHex("A67B5B") *  Colour4.FromHex("777777"),
                                TopRight = Colour4.FromHex("A67B5B")*  Colour4.FromHex("999999"),
                                BottomLeft = Colour4.FromHex("A67B5B")*  Colour4.FromHex("BBBBBB"),
                                BottomRight = Colour4.FromHex("A67B5B")*  Colour4.FromHex("CCCCCC")
                            }
                        },
                    }
                },
                loadingSpinner = new KCSLoadingSpinner()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(100f, 100f)
                }
            };
            loadingSpinner.Hide();

            splittableContainer.FirstContainer.Add(new LabeledContainer()
            {
                Label = "File Browser",
                RelativeSizeAxes = Axes.Both,
                Child = fileBrowserTreeView = new KCSTreeView()
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = Colour4.FromHex("09090A")
                }
            });

            splittableContainer.SecondContainer.Add(tabControlContainer = new KCSTabControlContainer()
            {
                RelativeSizeAxes = Axes.Both
            });

            TabPageContainer listViewFolderViewTab = new TabPageContainer("Folder View")
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f, 1f),
            };
            listViewFolderViewTab.Add(new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f),
                Colour = Colour4.FromHex("000000")
            });
            listViewFolderViewTab.Add(new DroppableContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f),
                Child = new GridContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding() { Horizontal = 10f, Vertical = 6f },
                    RowDimensions = new Dimension[]
                    {
                        new Dimension(mode: GridSizeMode.Absolute, size: 36f),
                        new Dimension(mode: GridSizeMode.Distributed),
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
                            folderView = new KCSListView()
                            {
                                RelativeSizeAxes = Axes.Both,
                                BackgroundColour = Colour4.FromHex("0C0C0C"),
                                HeaderColour = Colour4.FromHex("1A1A1A"),
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
                                Action = onBackBtnClicked
                            }
                        }
                    },
                    currentPathTextbox = new KCSTextbox()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        BackgroundColour = Colour4.FromHex("121212"),
                        Padding = new MarginPadding() { Horizontal = 5f },
                        Height = 30f,
                        CornerRadius = 10f,
                    },
                    searchingTextbox = new KCSTextbox()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        BackgroundColour = Colour4.FromHex("121212"),
                        Height = 30f,
                        CornerRadius = 10f,
                    }
                }
            };
            folderView.Headers.Add(new ListViewHeaderItem("Name", "Name", 0.5f));
            folderView.Headers.Add(new ListViewHeaderItem("Type", "Type", 0.3f));
            folderView.Headers.Add(new ListViewHeaderItem("Size", "Size", 0.2f));
            listViewFolderViewTab.Pinned.Value = true;
            folderView.ItemDoubleClicked += onFolderViewDoubleClicked;
            tabControlContainer.Add(listViewFolderViewTab);

            filePicker.Canceled += onFilePickerCanceled;
        }

        private MenuItem[] createMenuItems()
        {
            return new []
            {
                new MenuItem("File", () => { })
                {
                    Items = new[]
                    {
                        new MenuItem("Open", menuOpen),
                        new MenuItem("Open KartRider Data folder", menuOpenKartRiderDataFolder),
                        new MenuItem("Open RayCity Data folder", menuOpenRayCityDataFolder),
                        new MenuItem("Save", () => { }),
                        new MenuItem("Save As", () => { }),
                        new MenuItem("Save Workspace", () => { }),
                        new KCSMenuItemSpacer(),
                        new MenuItem("Exit", menuExit),
                    }
                },
                new MenuItem("Edit")
                {
                    Items = new[]
                    {
                        new MenuItem("Add File"),
                        new MenuItem("Remove All"),
                    }
                },
                new MenuItem("Extract")
                {
                    Items = new[]
                    {
                        new MenuItem("Extract selected file"),
                        new MenuItem("Extract current folder"),
                        new MenuItem("Extract all"),
                    }
                },
                new MenuItem("About")
                {
                    Items = new[]
                    {
                        new MenuItem("Checks updates"),
                        new MenuItem("Bug Report"),
                        new MenuItem("About"),
                    }
                },
            };
        }


        #endregion

        #region Event Handlers

        private void menuOpen()
        {
            filePicker.PickMode = FilePickMode.AllowFile;
            filePicker.FileSelected += onRhoJmdFileSelected;
            dialogContainer.Show();
        }

        private void menuOpenKartRiderDataFolder()
        {
            filePicker.PickMode = FilePickMode.AllowDirectory;
            filePicker.DirectorySelected += onKartDataFolderSelected;
            dialogContainer.Show();
        }

        private void menuOpenRayCityDataFolder()
        {
            filePicker.PickMode = FilePickMode.AllowDirectory;
            filePicker.DirectorySelected += onRcDataFolderSelected;
            dialogContainer.Show();
        }

        private void onRhoJmdFileSelected(FileInfo obj)
        {
            filePicker.FileSelected -= onRhoJmdFileSelected;
            dialogContainer.Hide();

            if (obj.Extension.ToLower() == ".rho")
            {
                archiveModel?.Dispose();
                archiveModel = new RhoArchiveModel(obj.FullName);
            }
            else if(obj.Extension.ToLower() == ".jmd")
            {
                // archiveModel = new JmdArchiveModel(obj.FullName);
            }
            else
            {
                throw new Exception();
            }

            if (archiveModel is not null)
            {
                archiveModel.BeginLoadArchive();
                Scheduler.AddOnce(loadingSpinner.Show);
                archiveModel.LoadCompleted += onArchiveModelLoadCompleted;
            }

        }

        private void onFilePickerCanceled()
        {
            filePicker.FileSelected -= onRhoJmdFileSelected;
            filePicker.DirectorySelected -= onKartDataFolderSelected;
            filePicker.DirectorySelected -= onRcDataFolderSelected;
            dialogContainer.Hide();
        }

        private void onKartDataFolderSelected(DirectoryInfo obj)
        {
            filePicker.DirectorySelected -= onKartDataFolderSelected;
            dialogContainer.Hide();

            archiveModel?.Dispose();
            archiveModel = new KartStorageArchiveModel(obj.FullName);
            if (archiveModel is not null)
            {
                archiveModel.BeginLoadArchive();
                Scheduler.AddOnce(loadingSpinner.Show);
                archiveModel.LoadCompleted += onArchiveModelLoadCompleted;
            }
        }

        private void onRcDataFolderSelected(DirectoryInfo obj)
        {
            filePicker.DirectorySelected -= onRcDataFolderSelected;
            dialogContainer.Hide();

            archiveModel?.Dispose();
            archiveModel = new RCStorageArchiveModel(obj.FullName);
            if (archiveModel is not null)
            {
                archiveModel.BeginLoadArchive();
                Scheduler.AddOnce(loadingSpinner.Show);
                archiveModel.LoadCompleted += onArchiveModelLoadCompleted;
            }
        }

        private void onArchiveModelLoadCompleted(bool success)
        {
            if (success)
            {
                currentFolder = archiveModel?.RootFolder;
                Scheduler.AddOnce(updateFileBrowser);
                Scheduler.AddOnce(updateFolderView);
                Scheduler.AddOnce(loadingSpinner.Hide);
            }
        }

        private void updateFileBrowser()
        {
            if (archiveModel is not null)
            {
                fileBrowserTreeView.Nodes.Clear();

                Queue<Tuple<TreeViewNode, IArchiveFolder>> bfsQueue = new Queue<Tuple<TreeViewNode, IArchiveFolder>>();
                bfsQueue.Enqueue(new (null, archiveModel.RootFolder));
                while (bfsQueue.Count > 0)
                {
                    var dequeueEle = bfsQueue.Dequeue();
                    foreach (IArchiveFolder folder in dequeueEle.Item2.Folders)
                    {
                        TreeViewNode childFolderNode = new TreeViewNode(
                            text: folder.Name,
                            iconName: "folder_close",
                            tag: folder,
                            clickAction: onTreeViewNodeClicked,
                            doubleClickAction: node => node.Expanded.Value ^= true
                        );
                        bfsQueue.Enqueue(new(childFolderNode, folder));
                        if (dequeueEle.Item1 is not null)
                        {
                            dequeueEle.Item1.Nodes.Add(childFolderNode);
                        }
                        else
                        {
                            fileBrowserTreeView.Nodes.Add(childFolderNode);
                        }
                    }

                    foreach (IArchiveFile file in dequeueEle.Item2.Files)
                    {
                        string ext = file.Name.Length > 4 ? file.Name[^4..] : "";
                        string iconName = getImageNameByExt(ext);
                        TreeViewNode childFileNode = new TreeViewNode(
                            text: file.Name,
                            iconName: iconName,
                            tag: file,
                            doubleClickAction: onFileTreeViewNodeDoubleClicked
                        );
                        if (dequeueEle.Item1 is not null)
                        {
                            dequeueEle.Item1.Nodes.Add(childFileNode);
                        }
                        else
                        {
                            fileBrowserTreeView.Nodes.Add(childFileNode);
                        }
                    }
                }
            }
        }

        private string getImageNameByExt(string ext)
        {
            return ext switch
            {
                ".png" => "file_image",
                ".jpg" => "file_image",
                ".tga" => "file_image",
                ".dds" => "file_image",
                ".jpeg" => "file_image",
                ".ksv" => "file_ksv",
                ".ogg" => "file_music",
                ".xml" => "file_xml",
                ".bml" => "file_xml",
                _ => "object_unknown",
                // _ => ""
            };
        }

        private void switchCurrentFolder(IArchiveFolder folder)
        {
            currentFolder = folder;
            currentPathTextbox.Text = folder?.FullName ?? "";
            Scheduler.AddOnce(updateFolderView);
        }

        private void updateFolderView()
        {
            backBtn.Enabled.Value = currentFolder?.Parent is not null;

            if (currentFolder is null)
                return;

            folderView.Items.Clear();
            foreach (IArchiveFolder childFolder in currentFolder.Folders)
            {
                folderView.Items.Add(new ListViewItem([
                    ("Name", childFolder.Name),
                    ("Type", "Folder"),
                    ("Size", ""),
                ], iconName: "folder_open")
                {
                    Tag = { Value = childFolder }
                });
            }

            foreach (IArchiveFile childFile in currentFolder.Files)
            {
                string ext = childFile.Name.Length > 4 ? childFile.Name[^4..] : "";
                string iconName = getImageNameByExt(ext);
                folderView.Items.Add(new ListViewItem([
                    ("Name", childFile.Name),
                    ("Type", "File"),
                    ("Size", ""),
                ], iconName: iconName)
                {
                    Tag = { Value = childFile }
                });
            }
        }

        private void onBackBtnClicked()
        {
            if(currentFolder?.Parent is not null)
                switchCurrentFolder(currentFolder.Parent);
        }

        private void onFolderViewDoubleClicked(ListViewItem item)
        {
            if (item.Tag.Value is IArchiveFile archiveFile)
            {
                createPreviewTab(archiveFile);
            }
            else if (item.Tag.Value is IArchiveFolder archiveFolder)
            {
                switchCurrentFolder(archiveFolder);
            }
        }

        private void onTreeViewNodeClicked(TreeViewNode node)
        {
            if (node.Tag is IArchiveFolder archiveFolder)
            {
                switchCurrentFolder(archiveFolder);
            }
        }

        private void onFileTreeViewNodeDoubleClicked(TreeViewNode obj)
        {
            if (obj.Tag is IArchiveFile archiveFile)
            {
                createPreviewTab(archiveFile);
            }
        }

        private void createPreviewTab(IArchiveFile archiveFile)
        {
            if (archiveFile.Name.EndsWith(".ogg"))
            {
                string displayName = archiveFile.Name[..^4];
                createMusicPreviewTab(archiveFile, displayName);
            }
            else if (Regex.IsMatch(archiveFile.Name, @"^.*\.(?:png|jpg|jpeg|tgs|dds)$"))
            {
                createImagePreviewTab(archiveFile);
            }
        }

        private void createMusicPreviewTab(IArchiveFile archiveFile, string displayName)
        {
            // TODO:
            MusicPreviewInfo? previewInfo = archiveModel?.MusicPreviewService.GetMusicPreviewInfo(archiveFile);
            string trackName = displayName;
            if (previewInfo is not null)
            {
                trackName = previewInfo.TrackName;
            }

            KCSMusicPlayer kcsPlayer;

            TabPageContainer pageContainer = new TabPageContainer(archiveFile.Name)
            {
                RelativeSizeAxes = Axes.Both,
                Size = new osuTK.Vector2(1, 1),
                Child = kcsPlayer = new KCSMusicPlayer(archiveFile.CreateStream(), archiveFile.FullName, trackName)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new osuTK.Vector2(1, 1),
                }
            };

            if (previewInfo?.BackgroundImage is not null)
            {
                kcsPlayer.BackgroundExtendContainer.Add(new RaycitySprite(previewInfo?.BackgroundImage)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                });
                kcsPlayer.BackgroundColour = new ColourInfo()
                {
                    TopLeft = Colour4.FromHex("000000FF"),
                    TopRight = Colour4.FromHex("000000FF"),
                    BottomRight = Colour4.FromHex("0000009A"),
                    BottomLeft =  Colour4.FromHex("0000009A"),
                };
            }

            tabControlContainer.Add(pageContainer);
            Scheduler.AddOnce(tabControlContainer.SwitchToTab, pageContainer);
        }

        private void createImagePreviewTab(IArchiveFile archiveFile)
        {
            TabPageContainer pageContainer = new TabPageContainer(archiveFile.Name)
            {
                RelativeSizeAxes = Axes.Both,
                Size = new osuTK.Vector2(1, 1),
                Child = new KCSImagePreview(archiveFile)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1.0f)
                }
            };
            tabControlContainer.Add(pageContainer);
        }

        private void menuExit()
        {
            this.Exit();
        }
        #endregion
    }
}
