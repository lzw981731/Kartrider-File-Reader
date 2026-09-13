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
using System.Resources;
using KartLibrary.Xml;
using KartLibrary.IO;
using KartLibrary.File;

using RhoLoader.PreviewWindow;
using RhoLoader.Setting;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;

using RhoLoader.Controls;

namespace RhoLoader
{
    public partial class MainWindow : Form
    {
        private SettingLoader BaseSettingLoader = new SettingLoader();

        private PackFolderManager BaseFolderManager = new PackFolderManager();

        private PackFolderInfo _root_folder;

        private PackFolderInfo _cur_folder;

        private enum OpenMode
        {
            None,
            SingleFile,
            MultipleFiles,
            DataFolder
        }

        private OpenMode _openMode = OpenMode.None;

        private class OpenedArchive
        {
            public string FilePath;
            public RhoArchive Archive;
        }

        private List<OpenedArchive> _openedArchives = new List<OpenedArchive>();

        public MainWindow()
        {
            InitializeComponent();
            LanguageManager.LoadLang();
            LanguageManager.LanguageName = "en-us";
            AddLanguages();
            LoadSetting();
            LoadLang();
            listview_main.SmallImageList = imageList_listview;
            listview_main.SmallImageList.Images.Add("file", new Bitmap(global::RhoLoader.Properties.Resources.baseline_insert_drive_file_black_18dp));
            listview_main.SmallImageList.Images.Add("folder", new Bitmap(global::RhoLoader.Properties.Resources.folder_close));
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
            this.menu_file_openFiles.Text = ((string)this.menu_file_openFiles.Tag).GetStringBag();
            this.menu_file_openFolder.Text = ((string)this.menu_file_openFolder.Tag).GetStringBag();
            this.menu_file_exit.Text = ((string)this.menu_file_exit.Tag).GetStringBag();
            this.menuToolStripMenuItem.Text = ((string)this.menuToolStripMenuItem.Tag).GetStringBag();
            this.menu_extract.Text = ((string)this.menu_extract.Tag).GetStringBag();
            this.menu_extract_all.Text = ((string)this.menu_extract_all.Tag).GetStringBag();
            this.menu_extract_current.Text = ((string)this.menu_extract_current.Tag).GetStringBag();
            this.filemenu_extractfile.Text = ((string)this.filemenu_extractfile.Tag).GetStringBag();
            this.menu_about.Text = ((string)this.menu_about.Tag).GetStringBag();
            this.columnHeader1.Text = ((string)this.columnHeader1.Tag).GetStringBag();
            this.columnHeader2.Text = ((string)this.columnHeader2.Tag).GetStringBag();
            this.columnHeader3.Text = ((string)this.columnHeader3.Tag).GetStringBag();
            this.Text = ((string)this.Tag).GetStringBag();
            this.menu_lang.Text = ((string)this.menu_lang.Tag).GetStringBag();
            this.filemenu_extract_selected.Text = ((string)this.filemenu_extract_selected.Tag).GetStringBag();
            this.filemenu_convertPNG.Text = ((string)this.filemenu_convertPNG.Tag).GetStringBag();
            this.filemenu_convertXML.Text = ((string)this.filemenu_convertXML.Tag).GetStringBag();
            LoadLangFont();
            if (_cur_folder is not null)
                UpdateUIFolder();
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
                menu_langname.Click += action_changeLanguage;
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
            this.menu_file_openFiles.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFiles.Font);
            this.menu_file_openFolder.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFolder.Font);
            this.menu_file_exit.Font = LanguageManager.GetLangFontWithBase(this.menu_file_exit.Font);
            this.menu_extract.Font = LanguageManager.GetLangFontWithBase(this.menu_extract.Font);
            this.menu_extract_all.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_all.Font);
            this.menu_extract_current.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_current.Font);
            this.filemenu_extractfile.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extractfile.Font);
            this.menu_about.Font = LanguageManager.GetLangFontWithBase(this.menu_about.Font);
            this.listview_main.Font = LanguageManager.GetLangFontWithBase(this.listview_main.Font);
            this.Text = ((string)this.Tag).GetStringBag();
            //this.menu_lang.Font = LanguageManager.GetLangFontWithBase(this.menu_lang.Font);
            this.filemenu_convertPNG.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertPNG.Font);
            this.filemenu_convertXML.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertXML.Font);
            this.filemenu_extract_selected.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extract_selected.Font);
        }

        #endregion
        #region Setting Loading
        private void LoadSetting()
        {
            if (!File.Exists("Setting.json"))
                return;
            BaseSettingLoader.LoadSetting("Setting.json");
            RhoLoader.Setting.Setting baseSetting = BaseSettingLoader.Setting;
            LanguageManager.SetLanguage(LanguageName: baseSetting.Language);
        }
        #endregion
        #region Control Action
        private void action_changeLanguage(object sender, EventArgs e)
        {
            ToolStripItem menu_langname = (ToolStripItem)sender;
            LanguageManager.SetLanguage(DisplayName: menu_langname.Text);
            BaseSettingLoader.Setting.Language = LanguageManager.LanguageName;
            BaseSettingLoader.SaveSetting("Setting.json");
            LoadLang();
        }
        private void action_openFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select Data folder including aaa.pk.";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists($"{fbd.SelectedPath}\\aaa.pk"))
                    MessageBox.Show("aaa.pk cannot found in the folder.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    CloseCurrentFile();
                    BaseFolderManager.OpenDataFolder($"{fbd.SelectedPath}\\aaa.pk");
                    _openMode = OpenMode.DataFolder;
                    _openedArchives.Clear();
                    Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
                    Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                    PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("");
                    foreach (PackFolderInfo folder in rootFolders)
                    {
                        folderQueue.Enqueue(folder);
                        TreeNode rootNode = new TreeNode()
                        {
                            Text = folder.FolderName,
                            Tag = new NodeInfoContainer(NodeType.Folder, folder)
                        };
                        nodeQueue.Enqueue(rootNode);
                        treeview_explorer.Nodes.Add(rootNode);
                    }
                    while (folderQueue.Count > 0 && nodeQueue.Count > 0)
                    {
                        TreeNode node = nodeQueue.Dequeue();
                        PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                        foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                        {
                            TreeNode subnode = new TreeNode()
                            {
                                Text = folder.FolderName,
                                Tag = new NodeInfoContainer(NodeType.Folder, folder)
                            };
                            node.Nodes.Add(subnode);
                            folderQueue.Enqueue(folder);
                            nodeQueue.Enqueue(subnode);
                        }
                    }
                    _cur_folder = _root_folder = BaseFolderManager.GetRootFolder();
                    //PathStack.Push("");
                    UpdateUIFolder();
                }
            }
        }
        private void action_open(object sender, EventArgs e)
        {
            dialog_multiFile.InitialDirectory = dialog_singleFile.InitialDirectory;
            if (dialog_singleFile.ShowDialog() == DialogResult.OK)
            {
                CloseCurrentFile();
                BaseFolderManager.OpenSingleFile(dialog_singleFile.FileName);
                // Open the underlying Rho archive (editable) for single-file mode
                _openMode = OpenMode.SingleFile;
                _openedArchives.Clear();
                try
                {
                    RhoArchive rhoArchive = new RhoArchive();
                    rhoArchive.Open(dialog_singleFile.FileName);
                    _openedArchives.Add(new OpenedArchive()
                    {
                        FilePath = dialog_singleFile.FileName,
                        Archive = rhoArchive
                    });
                }
                catch (Exception ex)
                {
                    _openMode = OpenMode.None;
                    MessageBox.Show($"Rho archive open failed: {ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
                Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("");
                foreach (PackFolderInfo folder in rootFolders)
                {
                    folderQueue.Enqueue(folder);
                    TreeNode rootNode = new TreeNode()
                    {
                        Text = folder.FolderName,
                        Tag = new NodeInfoContainer(NodeType.Folder, folder)
                    };
                    nodeQueue.Enqueue(rootNode);
                    treeview_explorer.Nodes.Add(rootNode);
                }
                while (folderQueue.Count > 0 && nodeQueue.Count > 0)
                {
                    TreeNode node = nodeQueue.Dequeue();
                    PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                    foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                    {
                        TreeNode subnode = new TreeNode()
                        {
                            Text = folder.FolderName,
                            Tag = new NodeInfoContainer(NodeType.Folder, folder)
                        };
                        node.Nodes.Add(subnode);
                        folderQueue.Enqueue(folder);
                        nodeQueue.Enqueue(subnode);
                    }
                }
                _cur_folder = _root_folder = rootFolders[0];
                //PathStack.Push(rootFolders[0].FullName);
                UpdateUIFolder();
            }
        }
        private void action_open_files(object sender, EventArgs e)
        {
            dialog_singleFile.InitialDirectory = dialog_multiFile.InitialDirectory;
            if (dialog_multiFile.ShowDialog() == DialogResult.OK)
            {
                CloseCurrentFile();
                BaseFolderManager.OpenMultipleFiles(dialog_multiFile.FileNames);
                _openMode = OpenMode.MultipleFiles;
                _openedArchives.Clear();
                foreach (string rhoPath in dialog_multiFile.FileNames)
                {
                    try
                    {
                        RhoArchive rhoArchive = new RhoArchive();
                        rhoArchive.Open(rhoPath);
                        _openedArchives.Add(new OpenedArchive()
                        {
                            FilePath = rhoPath,
                            Archive = rhoArchive
                        });
                    }
                    catch (Exception ex)
                    {
                        // Do not reset _openMode here: other files may have opened
                        // successfully. _openMode describes the UI mode as a whole.
                        MessageBox.Show($"Rho archive open failed: {ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
                Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("");
                foreach (PackFolderInfo folder in rootFolders)
                {
                    folderQueue.Enqueue(folder);
                    TreeNode rootNode = new TreeNode()
                    {
                        Text = folder.FolderName,
                        Tag = new NodeInfoContainer(NodeType.Folder, folder)
                    };
                    nodeQueue.Enqueue(rootNode);
                    treeview_explorer.Nodes.Add(rootNode);
                }
                while (folderQueue.Count > 0 && nodeQueue.Count > 0)
                {
                    TreeNode node = nodeQueue.Dequeue();
                    PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                    foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                    {
                        TreeNode subnode = new TreeNode()
                        {
                            Text = folder.FolderName,
                            Tag = new NodeInfoContainer(NodeType.Folder, folder)
                        };
                        node.Nodes.Add(subnode);
                        folderQueue.Enqueue(folder);
                        nodeQueue.Enqueue(subnode);
                    }
                }
                _cur_folder = _root_folder = BaseFolderManager.GetRootFolder();
                //PathStack.Push(rootFolders[0].FullName);
                UpdateUIFolder();
            }
        }
        private void action_aboutWindow(object sender, EventArgs e)
        {
            AboutMe aboutDialog = new AboutMe();
            aboutDialog.ShowDialog();
        }
        private void action_exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void action_save(object sender, EventArgs e)
        {
            if (_openedArchives.Count == 0)
            {
                MessageBox.Show(
                    "msg_open_plz".GetStringBag(),
                    "msg_level_error".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            try
            {
                foreach (OpenedArchive openedArchive in _openedArchives)
                {
                    openedArchive.Archive.SaveTo(openedArchive.FilePath);
                }
                // Reload the current file so the UI reflects the saved content
                string[] savedPaths = _openedArchives.Select(x => x.FilePath).ToArray();
                bool wasSingle = _openMode == OpenMode.SingleFile;
                CloseCurrentFile();
                if (wasSingle && savedPaths.Length > 0)
                {
                    action_open_saved(savedPaths[0]);
                }
                else if (savedPaths.Length > 0)
                {
                    action_open_saved_multiple(savedPaths);
                }
                MessageBox.Show(
                    "msg_save_done".GetStringBag(),
                    "msg_level_info".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed: {ex.Message}",
                    "msg_level_error".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void action_open_saved(string rhoPath)
        {
            BaseFolderManager.OpenSingleFile(rhoPath);
            _openMode = OpenMode.SingleFile;
            _openedArchives.Clear();
            RhoArchive rhoArchive = new RhoArchive();
            rhoArchive.Open(rhoPath);
            _openedArchives.Add(new OpenedArchive()
            {
                FilePath = rhoPath,
                Archive = rhoArchive
            });
            RebuildExplorerTreeFromRoot();
        }

        private void action_open_saved_multiple(string[] rhoPaths)
        {
            BaseFolderManager.OpenMultipleFiles(rhoPaths);
            _openMode = OpenMode.MultipleFiles;
            _openedArchives.Clear();
            foreach (string rhoPath in rhoPaths)
            {
                RhoArchive rhoArchive = new RhoArchive();
                rhoArchive.Open(rhoPath);
                _openedArchives.Add(new OpenedArchive()
                {
                    FilePath = rhoPath,
                    Archive = rhoArchive
                });
            }
            RebuildExplorerTreeFromRoot();
        }

        private void RebuildExplorerTreeFromRoot()
        {
            treeview_explorer.Nodes.Clear();
            Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
            Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
            PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("");
            foreach (PackFolderInfo folder in rootFolders)
            {
                folderQueue.Enqueue(folder);
                TreeNode rootNode = new TreeNode()
                {
                    Text = folder.FolderName,
                    Tag = new NodeInfoContainer(NodeType.Folder, folder)
                };
                nodeQueue.Enqueue(rootNode);
                treeview_explorer.Nodes.Add(rootNode);
            }
            while (folderQueue.Count > 0 && nodeQueue.Count > 0)
            {
                TreeNode node = nodeQueue.Dequeue();
                PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                {
                    TreeNode subnode = new TreeNode()
                    {
                        Text = folder.FolderName,
                        Tag = new NodeInfoContainer(NodeType.Folder, folder)
                    };
                    node.Nodes.Add(subnode);
                    folderQueue.Enqueue(folder);
                    nodeQueue.Enqueue(subnode);
                }
            }
            if (rootFolders.Length > 0)
                _cur_folder = _root_folder = rootFolders[0];
            else
                _cur_folder = _root_folder = BaseFolderManager.GetRootFolder();
            UpdateUIFolder();
        }
        private void action_listview_click(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            if (listview_main.SelectedItems.Count == 0)
                return;
            contextMenu_list.Show((Control)sender, e.X, e.Y);
            filemenu_extract_selected.Enabled = true;
            if (listview_main.SelectedItems.Count == 1 && listview_main.SelectedItems[0].Tag is PackFileInfo)
            {
                filemenu_extractfile.Enabled = true;
                filemenu_convertPNG.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".tga") || listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".dds");
                filemenu_convertXML.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".bml");
            }
            else
            {
                filemenu_extractfile.Enabled = false;
                filemenu_convertPNG.Enabled = false;
                filemenu_convertXML.Enabled = false;
            }
        }
        private void action_listview_doubleclick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = sel_item.Text;
                string ext = sel_file.FullName[^3..^0];
                if (ext == "dds" || ext == "tga")
                {
                    TgaDDsViewer tdv = new TgaDDsViewer();
                    tdv.Data = sel_file.GetData();
                    tdv.Type = ext == "dds" ? TgaDDsViewer.FileType.dds : ext == "tga" ? TgaDDsViewer.FileType.tga : throw new Exception();
                    tdv.ShowBox();
                }
                else if (ext == "bml")
                {
                    byte[] bmlData = sel_file.GetData();
                    bmlViewer bv = new bmlViewer(bmlData, sel_file.FullName);
                    bv.Show();
                }
                else if (ext == "ksv")
                {
                    byte[] ksvData = sel_file.GetData();
                    string region_str = LanguageManager.LanguageName switch
                    {
                        "ko-kr" => "kr",
                        "zh-cn" => "cn",
                        "zh-tw" => "tw",
                        _ => "kr"
                    };
                    PackFileInfo? track_info = BaseFolderManager.GetFile($"track_/common/trackLocale@{region_str}.bml");
                    if (track_info is not null)
                    {
                        byte[] data = track_info.GetData();
                        BinaryXmlDocument bmlDoc = new BinaryXmlDocument();
                        bmlDoc.Read(Encoding.GetEncoding("UTF-16"), data);
                        KSVPreview preview = new KSVPreview(ksvData, bmlDoc.RootTag);
                        preview.Show();
                    }
                    else
                    {
                        KSVPreview preview = new KSVPreview(ksvData);
                        preview.Show();
                    }
                }
                else
                {
                    if (ext == "kml")
                    {
                        FileName = $"{FileName[0..^3]}.xml";
                    }
                    FileStream fs = new FileStream(Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}", FileMode.Create);
                    byte[] data = sel_file.GetData();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                    Process ps = new Process();
                    ps.StartInfo.FileName = "explorer.exe";
                    ps.StartInfo.Arguments = Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}";
                    ps.Start();
                }
            }
            else if (sel_item.Tag is PackFolderInfo sel_folder)
            {
                _cur_folder = sel_folder;
                UpdateUIFolder();
            }
        }
        private void action_back(object sender, EventArgs e)
        {
            if (_cur_folder != _root_folder)
            {
                _cur_folder = _cur_folder.ParentFolder == null ? _root_folder : _cur_folder.ParentFolder;
                UpdateUIFolder();
            }
            else
            {
                icon_back.Enabled = false;
            }
        }
        private void action_icon_enable_changed(object sender, EventArgs e)
        {
            if (icon_back.Enabled)
                this.icon_back.Image = global::RhoLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled;
            else
                this.icon_back.Image = global::RhoLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled_disabled;
        }
        private void action_node_select(object sender, TreeViewEventArgs e)
        {
            var click_node = e.Node;
            if (click_node is null)
                return;
            var node_info = click_node.Tag as NodeInfoContainer;
            if (node_info is null)
                return;
            var folder_info = node_info.BaseData as PackFolderInfo;
            if (folder_info is null)
                return;
            _cur_folder = folder_info;
            UpdateUIFolder();
        }
        private void action_extract_all(object sender, EventArgs e)
        {
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_root_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extract_current(object sender, EventArgs e)
        {
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_cur_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extractfile(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                sfd.Filter = "AllFiles|*.*";
                sfd.FileName = FileName;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    byte[] data = sel_file.GetData();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                }
            }
        }
        private void action_extract_selected(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {
                List<PackFileInfo> output_files = new List<PackFileInfo>();
                List<PackFolderInfo> output_folders = new List<PackFolderInfo>();
                foreach (ListViewItem sel_item in listview_main.SelectedItems)
                {
                    if (sel_item.Tag is PackFileInfo sel_file)
                    {
                        output_files.Add((PackFileInfo)sel_file.Clone());
                    }
                    else if (sel_item.Tag is PackFolderInfo sel_folder)
                    {
                        output_folders.Add((PackFolderInfo)sel_folder.Clone());
                    }
                }
                PackFolderInfo extract_virtual_folder = new PackFolderInfo("", "", null, output_folders, output_files);
                ExtractFolder _extractFolderDialog = new ExtractFolder(extract_virtual_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_convert_png(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = sel_file.GetData();
                TgaDDsViewer tga_viewer = new TgaDDsViewer();
                tga_viewer.Data = data;
                tga_viewer.ConvertTGADDSToPng();
                data = null;
            }
        }
        private void action_convert_xml(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = sel_file.GetData();
                BinaryXmlDocument bxd = new BinaryXmlDocument();
                bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                string output = bxd.RootTag.ToString();
                byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = $"{sel_file.FullName[0..^3]}.xml";
                sfd.Filter = "XML File|*.xml";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(output_data, 0, output_data.Length);
                    fs.Close();
                }
            }
        }
        private void action_drag_enter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void action_drag_drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (droppedFiles == null || droppedFiles.Length == 0)
                return;

            bool hasOpenedArchive = BaseFolderManager.Initizated;
            bool allAreArchives = droppedFiles.Length > 0 && droppedFiles.All(f =>
                Path.GetExtension(f).Equals(".rho", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(f).Equals(".nho", StringComparison.OrdinalIgnoreCase));

            // Dragging .rho/.nho file(s) means "open this archive"
            if (allAreArchives)
            {
                if (droppedFiles.Length == 1)
                    action_open_saved(droppedFiles[0]);
                else
                    action_open_saved_multiple(droppedFiles);
                return;
            }

            if (!hasOpenedArchive)
            {
                MessageBox.Show(
                    "msg_open_plz".GetStringBag(),
                    "msg_level_error".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Determine target folder: treeview node under cursor, or current folder
            PackFolderInfo targetFolder = ResolveDropTargetFolder(sender, e);
            if (targetFolder == null)
            {
                MessageBox.Show(
                    "msg_open_plz".GetStringBag(),
                    "msg_level_error".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            int addedCount = 0;
            int skippedCount = 0;

            foreach (string droppedPath in droppedFiles)
            {
                if (System.IO.File.Exists(droppedPath))
                {
                    string fileName = Path.GetFileName(droppedPath);

                    // Check if file with same name already exists in target folder
                    bool exists = false;
                    foreach (PackFileInfo existingFile in targetFolder.GetFilesInfo())
                    {
                        if (existingFile.FileName == fileName)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                    {
                        skippedCount++;
                        continue;
                    }

                    FileInfo fi = new FileInfo(droppedPath);
                    PackFileInfo newFileInfo = new PackFileInfo()
                    {
                        FileName = fileName,
                        FullName = targetFolder.FullName == "" ? fileName : $"{targetFolder.FullName}/{fileName}",
                        FileSize = (int)fi.Length,
                        PackFileType = PackFileType.ExternalFile,
                        OriginalFile = droppedPath
                    };
                    targetFolder.Files.Add(newFileInfo);
                    // Write the dropped file into the underlying Rho archive structure
                    AddFileToArchive(targetFolder, droppedPath, fileName);
                    addedCount++;
                }
                else if (Directory.Exists(droppedPath))
                {
                    string folderName = Path.GetFileName(droppedPath);
                    bool exists = false;
                    foreach (PackFolderInfo existingFolder in targetFolder.GetFoldersInfo())
                    {
                        if (existingFolder.FolderName == folderName)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                    {
                        skippedCount++;
                        continue;
                    }

                    PackFolderInfo newFolder = new PackFolderInfo()
                    {
                        FolderName = folderName,
                        FullName = targetFolder.FullName == "" ? folderName : $"{targetFolder.FullName}/{folderName}",
                        ParentFolder = targetFolder
                    };

                    // Recursively add files from dropped directory
                    AddDirectoryContents(newFolder, droppedPath);
                    targetFolder.Folders.Add(newFolder);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                // If files were dropped onto a treeview node, rebuild the tree so new folders appear;
                // otherwise just refresh the current list view.
                if (sender is DarkTreeView)
                    RebuildExplorerTree(targetFolder);
                else
                    UpdateUIFolder();
            }

            if (skippedCount > 0)
            {
                MessageBox.Show(
                    string.Format("msg_drag_skipped".GetStringBag(), skippedCount),
                    "msg_level_warning".GetStringBag(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private PackFolderInfo? ResolveDropTargetFolder(object sender, DragEventArgs e)
        {
            if (sender is DarkTreeView)
            {
                TreeNode? hitNode = treeview_explorer.GetNodeAt(e.X, e.Y);
                if (hitNode is null)
                    hitNode = treeview_explorer.GetNodeAt(treeview_explorer.PointToClient(new Point(e.X, e.Y)));
                if (hitNode?.Tag is NodeInfoContainer nodeInfo && nodeInfo.BaseData is PackFolderInfo folderInfo)
                    return folderInfo;
                // Dropped on treeview empty area: fall back to the current folder
                return _cur_folder;
            }
            return _cur_folder;
        }

        private void RebuildExplorerTree(PackFolderInfo selectedFolder)
        {
            treeview_explorer.Nodes.Clear();
            Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
            Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
            PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("");
            foreach (PackFolderInfo folder in rootFolders)
            {
                folderQueue.Enqueue(folder);
                TreeNode rootNode = new TreeNode()
                {
                    Text = folder.FolderName,
                    Tag = new NodeInfoContainer(NodeType.Folder, folder)
                };
                nodeQueue.Enqueue(rootNode);
                treeview_explorer.Nodes.Add(rootNode);
            }
            while (folderQueue.Count > 0 && nodeQueue.Count > 0)
            {
                TreeNode node = nodeQueue.Dequeue();
                PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                {
                    TreeNode subnode = new TreeNode()
                    {
                        Text = folder.FolderName,
                        Tag = new NodeInfoContainer(NodeType.Folder, folder)
                    };
                    node.Nodes.Add(subnode);
                    folderQueue.Enqueue(folder);
                    nodeQueue.Enqueue(subnode);
                }
            }
            // Navigate to the folder the user dropped onto
            if (selectedFolder is not null)
            {
                _cur_folder = selectedFolder;
                SelectTreeNode(treeview_explorer.Nodes, selectedFolder);
                UpdateUIFolder();
            }
        }

        private bool SelectTreeNode(TreeNodeCollection nodes, PackFolderInfo targetFolder)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is NodeInfoContainer nodeInfo && nodeInfo.BaseData is PackFolderInfo folderInfo)
                {
                    if (folderInfo.FullName == targetFolder.FullName)
                    {
                        treeview_explorer.SelectedNode = node;
                        node.Expand();
                        return true;
                    }
                }
                if (SelectTreeNode(node.Nodes, targetFolder))
                    return true;
            }
            return false;
        }

        private void AddDirectoryContents(PackFolderInfo targetFolder, string directoryPath)
        {
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                PackFileInfo fileInfo = new PackFileInfo()
                {
                    FileName = fi.Name,
                    FullName = targetFolder.FullName == "" ? fi.Name : $"{targetFolder.FullName}/{fi.Name}",
                    FileSize = (int)fi.Length,
                    PackFileType = PackFileType.ExternalFile,
                    OriginalFile = fi.FullName
                };
                targetFolder.Files.Add(fileInfo);
                AddFileToArchive(targetFolder, fi.FullName, fi.Name);
            }
            foreach (DirectoryInfo subDi in di.GetDirectories())
            {
                PackFolderInfo subFolder = new PackFolderInfo()
                {
                    FolderName = subDi.Name,
                    FullName = targetFolder.FullName == "" ? subDi.Name : $"{targetFolder.FullName}/{subDi.Name}",
                    ParentFolder = targetFolder
                };
                AddDirectoryContents(subFolder, subDi.FullName);
                targetFolder.Folders.Add(subFolder);
                AddFolderToArchive(targetFolder, subDi.Name);
            }
        }

        private void AddFileToArchive(PackFolderInfo targetFolder, string filePath, string fileName)
        {
            RhoFolder? rhoFolder = ResolveRhoFolder(targetFolder);
            if (rhoFolder is null)
                return;
            if (rhoFolder.ContainsFile(fileName))
                return;
            RhoFile newRhoFile = new RhoFile();
            newRhoFile.Name = fileName;
            newRhoFile.DataSource = new FileDataSource(filePath);
            newRhoFile.FileEncryptionProperty = RhoFileProperty.CompressedEncrypted;
            rhoFolder.AddFile(newRhoFile);
        }

        private void AddFolderToArchive(PackFolderInfo targetFolder, string folderName)
        {
            RhoFolder? rhoFolder = ResolveRhoFolder(targetFolder);
            if (rhoFolder is null)
                return;
            if (rhoFolder.ContainsFolder(folderName))
                return;
            RhoFolder newRhoFolder = new RhoFolder();
            newRhoFolder.Name = folderName;
            rhoFolder.AddFolder(newRhoFolder);
        }

        private RhoFolder? ResolveRhoFolder(PackFolderInfo targetFolder)
        {
            if (_openedArchives.Count == 0)
                return null;
            string targetFullName = targetFolder.FullName ?? "";

            // Match the target folder to one of the opened archives. Each opened
            // archive corresponds to a top-level PackFolderInfo whose FullName is
            // the rho file name (e.g. "aaa.rho").
            OpenedArchive? matchedArchive = null;
            string archiveRootName = "";

            if (_openMode == OpenMode.MultipleFiles)
            {
                foreach (OpenedArchive oa in _openedArchives)
                {
                    string rootName = Path.GetFileName(oa.FilePath);
                    if (targetFullName == rootName || targetFullName.StartsWith(rootName + "/"))
                    {
                        matchedArchive = oa;
                        archiveRootName = rootName;
                        break;
                    }
                }
            }
            else
            {
                matchedArchive = _openedArchives[0];
                archiveRootName = Path.GetFileName(_openedArchives[0].FilePath);
            }

            if (matchedArchive is null)
                return null;

            RhoFolder rootFolder = matchedArchive.Archive.RootFolder;
            if (targetFullName == archiveRootName)
                return rootFolder;

            string relativePath;
            if (targetFullName.StartsWith(archiveRootName + "/"))
                relativePath = targetFullName.Substring(archiveRootName.Length + 1);
            else if (archiveRootName == "")
                relativePath = targetFullName;
            else
                return null;

            return rootFolder.GetFolder(relativePath);
        }

        #endregion
        #region Other Function
        private string GetCurrentPath()
        {
            if (_cur_folder is null)
                return "";
            return _cur_folder.FullName;
        }
        private void UpdateUIFolder()
        {
            if (_cur_folder is null)
            {
                return;
            }
            List<ListViewItem> temp_list = new List<ListViewItem>();
            this.listview_main.Items.Clear();
            foreach (PackFolderInfo sub_folder in _cur_folder.GetFoldersInfo())
            {
                ListViewItem lvi = new ListViewItem(new string[] { sub_folder.FolderName, ("listview_item2_folder").GetStringBag(), $"" });
                lvi.ImageKey = "folder";
                lvi.Tag = sub_folder;
                temp_list.Add(lvi);
            }
            foreach (PackFileInfo sub_file in _cur_folder.GetFilesInfo())
            {
                ListViewItem lvi = new ListViewItem(new string[] { sub_file.FileName, ("listview_item2_file").GetStringBag(), $"{FormatDataLength(sub_file.FileSize)}" });
                lvi.ImageKey = "file";
                lvi.Tag = sub_file;
                temp_list.Add(lvi);
            }
            this.listview_main.Items.AddRange(temp_list.ToArray());
            this.textbox_path.Text = GetCurrentPath();
            if (_cur_folder != _root_folder)
                this.icon_back.Enabled = true;
            else
                this.icon_back.Enabled = false;
        }
        private void CloseCurrentFile()
        {
            /*
            FolderStack.Clear();
            PathStack.Clear();
            */
            treeview_explorer.Nodes.Clear();
            listview_main.Items.Clear();
            BaseFolderManager.Reset();
            foreach (OpenedArchive openedArchive in _openedArchives)
            {
                openedArchive.Archive.Dispose();
            }
            _openedArchives.Clear();
            _openMode = OpenMode.None;
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
