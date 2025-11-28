using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing.Text;
using System.Resources;
using KartLibrary.Xml;
using KartLibrary.IO;
using KartLibrary.File;

using RhoLoader.PreviewWindow;
using RhoLoader.Setting;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using KartCity.Common.Xml;
using KartCityStudio.Common.Model.Archive;
using KartCityStudio.Common.Model.Archive.Implements.Common;
using KartCityStudio.Common.Model.Archive.Implements.Jmd;
using KartCityStudio.Common.Model.Archive.Implements.KartStorage;
using KartCityStudio.Common.Model.Archive.Implements.Rho;
using KartCityStudio.Game.Model;
using KartCityStudio.Model.Archive.Implements.RCStorage;
using RhoLoader.Controls;
using RhoLoader.Controls.PreviewPanel;
using RhoLoader.Dialog;


namespace RhoLoader
{
    public partial class MainWindow : Form
    {
        private SettingLoader _baseSettingLoader = new SettingLoader();
        
        private IArchiveModel? _currentArchiveModel = null;
        private IArchiveFolder? _currentFolder = null;
        
        private LoadingDialog _dialogLoading =  new LoadingDialog();
        
        public MainWindow()
        {
            InitializeComponent();
            FontManager.Initialize();
            LanguageManager.LoadLang();
            LanguageManager.LanguageName = "en-us";
            AddLanguages();
            LoadSetting();
            LoadLang();
            _listviewMain.SmallImageList = imageList_listview;
            _listviewMain.SmallImageList.Images.Add("file", new Bitmap(global::RhoLoader.Properties.Resources.baseline_insert_drive_file_black_18dp));
            _listviewMain.SmallImageList.Images.Add("file_image", new Bitmap(global::RhoLoader.Properties.Resources.file_image));
            _listviewMain.SmallImageList.Images.Add("file_music", new Bitmap(global::RhoLoader.Properties.Resources.file_music));
            _listviewMain.SmallImageList.Images.Add("file_xml", new Bitmap(global::RhoLoader.Properties.Resources.file_xml));
            _listviewMain.SmallImageList.TransparentColor = Color.Transparent;
            _listviewMain.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
            _listviewMain.SmallImageList.ImageSize = new  Size(14, 14);
            _listviewMain.SmallImageList.Images.Add("folder", new Bitmap(global::RhoLoader.Properties.Resources.folder_close));
        }
        public MainWindow(StartupOption startupOption) : this()
        {
            
        }

        #region Globalization
        private void LoadLang()
        {
            this.menu.Text = ((string)this.menu.Tag).GetStringBag();
            
            this.menu_file.Text = ((string)this.menu_file.Tag).GetStringBag();
            this.menu_file_open.Text = ((string)this.menu_file_open.Tag).GetStringBag();
            this.menu_file_openFolderKr.Text = ((string)this.menu_file_openFolderKr.Tag).GetStringBag();
            this.menu_file_openFolderRc.Text = ((string)this.menu_file_openFolderRc.Tag).GetStringBag();
            this.menu_file_exit.Text = ((string)this.menu_file_exit.Tag).GetStringBag();
            this.menu_extract.Text = ((string)this.menu_extract.Tag).GetStringBag();
            this.menu_extract_all.Text = ((string)this.menu_extract_all.Tag).GetStringBag();
            this.menu_extract_current.Text = ((string)this.menu_extract_current.Tag).GetStringBag();
            this.fileMenuExtractfile.Text = ((string)this.fileMenuExtractfile.Tag).GetStringBag();
            this.menu_about.Text = ((string)this.menu_about.Tag).GetStringBag();
            this.columnHeader1.Text = ((string)this.columnHeader1.Tag).GetStringBag();
            this.columnHeader2.Text = ((string)this.columnHeader2.Tag).GetStringBag();
            this.columnHeader3.Text = ((string)this.columnHeader3.Tag).GetStringBag();
            
            this.Text = ((string)this.Tag).GetStringBag() +  RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => RuntimeInformation.OSArchitecture == Architecture.X64 ? " (X64)" : " (emulated X64 )",
                Architecture.Arm64 => " (Arm64)",
                Architecture.LoongArch64 => " (LoongArch64)",
                _ => " (Unknown)"
            };;
            this.menu_lang.Text = ((string)this.menu_lang.Tag).GetStringBag();
            this.fileMenuExtractSelected.Text = ((string)this.fileMenuExtractSelected.Tag).GetStringBag();
            this.fileMenuConvertPNG.Text = ((string)this.fileMenuConvertPNG.Tag).GetStringBag();
            this.fileMenuConvertXML.Text = ((string)this.fileMenuConvertXML.Tag).GetStringBag();
            LoadLangFont();
            UpdateListView().Wait();
            /*
            if (!(BaseRhoFile is null))
                UpdateFolders();
            */
        }
        private void AddLanguages()
        {
            List<ToolStripItem> temp = new List<ToolStripItem>();
            Language[] langs = LanguageManager.ListLanguages();
            foreach (Language lang in langs)
            {
                ToolStripMenuItem menu_langname = new ToolStripMenuItem();
                menu_langname.Text = lang.DisplayName;
                menu_langname.AutoSize = true;
                menu_langname.Tag = lang;
                menu_langname.Click += ActionChangeLanguage;
                menu_langname.Font = lang.GetLangFontWithBase(this.Font);
                temp.Add(menu_langname);
            }
            menu_lang.DropDownItems.AddRange(temp.ToArray());
        }
        private void LoadLangFont()
        {
            this.menu.Font = LanguageManager.GetLangFontWithBase(this.menu.Font);
            this.menu_file.Font = LanguageManager.GetLangFontWithBase(this.menu_file.Font);
            this.menu_file_open.Font = LanguageManager.GetLangFontWithBase(this.menu_file_open.Font);
            this.menu_file_openFolderKr.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFolderKr.Font);
            this.menu_file_openFolderRc.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFolderRc.Font);
            this.menu_file_exit.Font = LanguageManager.GetLangFontWithBase(this.menu_file_exit.Font);
            this.menu_extract.Font = LanguageManager.GetLangFontWithBase(this.menu_extract.Font);
            this.menu_extract_all.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_all.Font);
            this.menu_extract_current.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_current.Font);
            this.fileMenuExtractfile.Font = LanguageManager.GetLangFontWithBase(this.fileMenuExtractfile.Font);
            this.menu_about.Font = LanguageManager.GetLangFontWithBase(this.menu_about.Font);
            this._listviewMain.Font = LanguageManager.GetLangFontWithBase(this._listviewMain.Font);
            
            //this.menu_lang.Font = LanguageManager.GetLangFontWithBase(this.menu_lang.Font);
            this.fileMenuConvertPNG.Font = LanguageManager.GetLangFontWithBase(this.fileMenuConvertPNG.Font);
            this.fileMenuConvertXML.Font = LanguageManager.GetLangFontWithBase(this.fileMenuConvertXML.Font);
            this.fileMenuExtractSelected.Font = LanguageManager.GetLangFontWithBase(this.fileMenuExtractSelected.Font);
        }

        #endregion
        #region Setting Loading
        private void LoadSetting()
        {
            if (!File.Exists("Setting.json"))
                return;
            _baseSettingLoader.LoadSetting("Setting.json");
            RhoLoader.Setting.Setting baseSetting = _baseSettingLoader.Setting;
            LanguageManager.SetLanguage(LanguageName: baseSetting.Language);
        }

        private void SaveSetting()
        {
            _baseSettingLoader.SaveSetting("Setting.json");
        }
        #endregion

        #region Font Loading

        private void LoadFont()
        {
            PrivateFontCollection privateFontCollection = new PrivateFontCollection();
            foreach (var file in Directory.GetFiles("Fonts", "*.ttf"))
            {
                privateFontCollection.AddFontFile(file);    
            }
        }

        #endregion
        #region Control Action
        private void ActionChangeLanguage(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is Language lang)
            {
                LanguageManager.SetLanguage(LanguageName: lang.LanguageName);
                _baseSettingLoader.Setting.Language = lang.LanguageName;
                SaveSetting();
                LoadLang();
            }
        }
        private void ActionOpenFolderKr(object sender, EventArgs e)
        {
            if (_dialogKartData.ShowDialog() == DialogResult.OK)
            {
                _currentArchiveModel?.Dispose();
                _currentArchiveModel = new KartStorageArchiveModel(_dialogKartData.SelectedPath);
                _currentArchiveModel.LoadCompleted += ArchiveLoadComplete;
                _currentArchiveModel.LoadFailure += ArchiveLoadFailure;
                
                CancellationTokenSource cancellationTokenSource = new();
                _currentArchiveModel.BeginLoadArchive(cancellationTokenSource.Token);
                
                if (_dialogLoading.ShowDialog() != DialogResult.OK)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        }

        private void ActionOpenFolderRc(object sender, EventArgs e)
        {
            if (_dialogKartData.ShowDialog() == DialogResult.OK)
            {
                _currentArchiveModel?.Dispose();
                _currentArchiveModel = new RCStorageArchiveModel(_dialogKartData.SelectedPath);
                _currentArchiveModel.LoadCompleted += ArchiveLoadComplete;
                _currentArchiveModel.LoadFailure += ArchiveLoadFailure;
                
                CancellationTokenSource cancellationTokenSource = new();
                _currentArchiveModel.BeginLoadArchive(cancellationTokenSource.Token);
                
                if (_dialogLoading.ShowDialog() != DialogResult.OK)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        }
        private async void ActionOpen(object sender, EventArgs e)
        {
            if (_dialogMultiFile.ShowDialog() == DialogResult.OK)
            {
                _currentArchiveModel?.Dispose();

                AggregatedArchiveModel archiveModel  = new();
                foreach (var fileName in _dialogMultiFile.FileNames)
                {
                    if(fileName.EndsWith(".rho"))
                        archiveModel.MountArchive(Path.GetFileName(fileName), new RhoArchiveModel(fileName));
                    else if(fileName.EndsWith(".jmd"))
                        archiveModel.MountArchive(Path.GetFileName(fileName), new JmdArchiveModel(fileName));
                }
                _currentArchiveModel = archiveModel;
                _currentArchiveModel.LoadCompleted += ArchiveLoadComplete;
                _currentArchiveModel.LoadFailure += ArchiveLoadFailure;
                
                CancellationTokenSource cancellationTokenSource = new();
                _currentArchiveModel.BeginLoadArchive(cancellationTokenSource.Token);

                if (_dialogLoading.ShowDialog() != DialogResult.OK)
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
        }
        private void ActionAboutWindow(object sender, EventArgs e)
        {
            AboutMe aboutMe = new();
            aboutMe.ShowDialog();
        }
        private void ActionExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private async void ActionListViewClick(object sender, MouseEventArgs e)
        {
            
        }
        private async void ActionListviewDoubleclick(object sender, MouseEventArgs e)
        {
            if (_listviewMain.SelectedItems.Count == 1 && _currentFolder is not null)
            {
                if (_listviewMain.SelectedItems[0].Tag is IArchiveFolder archiveFolder)
                {
                    await EnterToFolder(archiveFolder);
                }
                else if (_listviewMain.SelectedItems[0].Tag is IArchiveFile archiveFile)
                {
                    await PreviewArchiveFile(archiveFile);
                }
            }
        }
        private async void ActionBack(object sender, EventArgs e)
        {
            if(_currentFolder?.Parent is not null)
            {
                await EnterToFolder(_currentFolder.Parent);   
            }
        }
        private void ActionIconEnableChanged(object sender, EventArgs e)
        {
            if (_iconBack.Enabled)
                this._iconBack.Image = global::RhoLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled;
            else
                this._iconBack.Image = global::RhoLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled_disabled;
        }
        private async void ActionNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (_treeViewExplorer.SelectedNode is not null &&
                _treeViewExplorer.SelectedNode.Tag is NodeInfoContainer nodeInfoContainer &&
                nodeInfoContainer.BaseData is IArchiveFolder archiveFolder)
            {
                await EnterToFolder(archiveFolder);
            }
        }
        private void ActionExtractAll(object sender, EventArgs e)
        {
            if (_currentArchiveModel is null)
            {
                MessageBox.Show("msg_PlzOpenFileFirst".GetStringBag(), "title".GetStringBag());
                return;
            }

            ExtractOption extractOptionDialog =  new ExtractOption();
            if (_dialogExtractSelector.ShowDialog() == DialogResult.OK && extractOptionDialog.ShowDialog() == DialogResult.OK)
            {
                ExtractFolder extractFolderDialog = new ExtractFolder(
                    _dialogExtractSelector.SelectedPath,
                    extractOptionDialog.SelectOption,
                    _currentArchiveModel.RootFolder
                );
                extractFolderDialog.ShowDialog();
            }
        }
        private void ActionExtractCurrent(object sender, EventArgs e)
        {
            if (_currentArchiveModel is null)
            {
                MessageBox.Show("msg_PlzOpenFileFirst".GetStringBag(), "title".GetStringBag());
                return;
            }

            if (_listviewMain.SelectedItems.Count > 0)
            {
                List<IArchiveElement> elements = [];
                foreach (ListViewItem selectedItem in _listviewMain.SelectedItems)
                {
                    if(selectedItem.Tag is IArchiveElement archiveElement)
                        elements.Add(archiveElement);
                }
                
                ExtractOption extractOptionDialog =  new ExtractOption();
                if (_dialogExtractSelector.ShowDialog() == DialogResult.OK && extractOptionDialog.ShowDialog() == DialogResult.OK)
                {
                    ExtractFolder extractFolderDialog = new ExtractFolder(
                        _dialogExtractSelector.SelectedPath,
                        extractOptionDialog.SelectOption,
                        [..elements]
                    );
                    extractFolderDialog.ShowDialog();
                }
            }
        }
        private void ActionExtractfile(object sender, EventArgs e)
        {
            
        }
        private void ActionExtractSelected(object sender, EventArgs e)
        {
            
        }
        private void ActionConvertPng(object sender, EventArgs e)
        {
            
        }
        private void ActionConvertXml(object sender, EventArgs e)
        {
            LoadingDialog loadingDialog = new LoadingDialog();
            loadingDialog.ShowDialog();
        }

        private void ActionDebug(object sender, EventArgs e)
        {
            TestWindow testWindow = new TestWindow();
            testWindow.Show();
        }

        private void ActionSelectItemChanged(object sender, EventArgs e)
        {
            if (_listviewMain.SelectedItems.Count > 0)
            {
                var selectedItem = _listviewMain.SelectedItems[0];
                if (selectedItem.Tag is IArchiveFile archiveFile)
                {
                    _previewPanel.LoadPreview(archiveFile);
                }
            }
        }

        private Control? ActionCreatePreviewControl(string extension, CancellationToken token)
        {
            switch (extension)
            {
                case ".xml":
                case ".bml":
                case ".kml":
                    return new XmlPreview();
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".dds":
                case ".tga":
                    return new ImagePreview();
            }

            return null;
        }

        private void ActionInitPreviewControl(IArchiveFile archiveFile, Control? control, Stream stream,
            CancellationToken token)
        {
            switch (control)
            {
                case XmlPreview xmlPreview:
                    bool isBml = (archiveFile.Name.EndsWith(".bml"));
                    xmlPreview.InitControl(stream, isBml, token);
                    break;
                case ImagePreview imagePreview:
                    bool reqConvert = (archiveFile.Name.EndsWith(".dds") || archiveFile.Name.EndsWith(".tga"));
                    imagePreview.InitControl(stream, reqConvert, token);
                    break;
            }
        }
        #endregion

        #region Other Function

        private void ArchiveLoadComplete(bool success)
        {
            _dialogLoading.LoadCompleted();
            
            if (success && _currentArchiveModel is not null)
            {
                Task.Run(async () =>
                {
                    await EnterToFolder(_currentArchiveModel.RootFolder);
                    await UpdateTreeNodes();
                });
            }
        }
        
        

        private void ArchiveLoadFailure(Exception exception)
        {
            if (this.InvokeRequired)
                this.Invoke(ArchiveLoadFailure, exception);
            else
                MessageBox.Show($"{exception.Message}\r\n" +
                                $"Exception: {exception.GetType()}\r\n" +
                                $"Stack trace: \r\n{exception.StackTrace??""}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private async Task UpdateListView()
        {
            if (this.InvokeRequired)
            {
                await this.Invoke(UpdateListView);
                return;
            }
            _listviewMain.Items.Clear();
            
            if (_currentFolder is not null)
            {
                _listviewMain.Items.AddRange(_currentFolder.Folders
                    .Select(x => new ListViewItem(new string[]{ x.Name, ("listview_item2_folder").GetStringBag(), "" })
                    {
                        ImageKey = "folder",
                        Tag = x,
                    })
                    .Concat(_currentFolder.Files.Select(x => new ListViewItem(new string[] { x.Name, ("listview_item2_file").GetStringBag(), FormatDataLength(x.FileSize) })
                    {
                        ImageKey = Path.GetExtension(x.Name) switch
                        {
                            ".ksv" => "file_ksv",
                            ".bml" => "file_xml",
                            ".xml" => "file_xml",
                            ".kml" => "file_xml",
                            ".png" => "file_image",
                            ".dds" => "file_image",
                            ".tga" => "file_image",
                            ".ogg" => "file_music",
                            _ => "file",
                        },
                        Tag = x,
                    })).ToArray());
            }
        }
        private async Task UpdateTreeNodes()
        {
            if (_currentArchiveModel is not null)
            {
                await Task.Run(() =>
                {
                    Queue<(TreeNode?, IArchiveFolder)> queue = [];
                    List<TreeNode> rootNodes = [];

                    queue.Enqueue((null, _currentArchiveModel.RootFolder));

                    while (queue.Count > 0)
                    {
                        var topElement = queue.Dequeue();
                        foreach (var folder in topElement.Item2.Folders)
                        {
                            TreeNode folderNode = new TreeNode()
                            {
                                Text = folder.Name,
                                Tag = new NodeInfoContainer(NodeType.Folder, folder)
                            };
                            if (topElement.Item1 is null)
                            {
                                rootNodes.Add(folderNode);
                            }
                            else
                            {
                                topElement.Item1.Nodes.Add(folderNode);
                            }
                            queue.Enqueue((folderNode, folder));
                        }
                    }

                    if (this.InvokeRequired)
                    {
                        this.Invoke(_treeViewExplorer.Nodes.Clear, []);
                        this.Invoke(_treeViewExplorer.Nodes.AddRange, [rootNodes.ToArray()]);
                    }
                    else
                    {
                        _treeViewExplorer.Nodes.Clear();
                        _treeViewExplorer.Nodes.AddRange([.. rootNodes]);
                    }
                });
            }
        }
        private string FormatDataLength(int length)
        {
            string[] units = { "Bytes", "KiB", "MiB" };
            double dlen = length;
            
            foreach (string unit in units)
            {
                if (dlen > 1024)
                    dlen /= 1024;
                else
                    return $"{dlen: 0.00} {unit}";
            }
            return $"{dlen} {units[units.Length - 1]}";
        }
        private async Task EnterToFolder(IArchiveFolder folder)
        {
            if (this.InvokeRequired)
            {
                await this.Invoke(async () => await EnterToFolder(folder));
                return;
            }
            _currentFolder = folder;
            textbox_path.Text = folder.FullName;
            _iconBack.Enabled = _currentFolder?.Parent is not null;
            
            await UpdateListView();
        }

        private async Task PreviewArchiveFile(IArchiveFile file)
        {
            await using var stream = file.CreateStream() ;
            string extension = Path.GetExtension(file.Name);

            if (extension is ".bml" or ".xml" or ".kml")
            {
                XmlViewer bmlViewer = new XmlViewer();
                if (extension is ".bml")
                {
                    BinaryReader reader = new BinaryReader(stream);
                    var bmlTag = reader.ReadBinaryXmlTag(Encoding.Unicode);
                    bmlViewer.LoadFromBml(file.Name, bmlTag);
                }
                else
                {
                    StreamReader streamReader = new StreamReader(stream);
                    var xml = await streamReader.ReadToEndAsync();
                    bmlViewer.LoadFromXml(file.Name, xml);
                }

                bmlViewer.Show();
            }
            else if (extension is ".dds" or ".tga")
            {
                await using var fileStream = file.CreateStream();
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer);

                ImageViewer viewer = new ImageViewer()
                {
                    Data = buffer
                };

                if (this.InvokeRequired)
                    this.Invoke(viewer.ShowBox);
                else
                {
                    viewer.ShowBox();
                }
            }
            else
            {
                string tmpFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                if(!Directory.Exists(tmpFilePath))
                    Directory.CreateDirectory(tmpFilePath);
                tmpFilePath = Path.Combine(tmpFilePath, file.Name);
                
                await using var fileStream = File.Create(tmpFilePath);
                await stream.CopyToAsync(fileStream);
                Process.Start("explorer.exe", tmpFilePath);
            }
        }

        #endregion

        #region StartupSetting
        public class StartupOption
        {
            public string FileName { get; set; } = "";
            public string DataFolderPath { get; set; } = "";
        }
        #endregion
    }
}
