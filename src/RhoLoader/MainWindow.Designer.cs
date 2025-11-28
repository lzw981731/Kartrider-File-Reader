using RhoLoader;
using RhoLoader.Controls;
using RhoLoader.Controls.PreviewPanel;

namespace RhoLoader
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menu = new System.Windows.Forms.MenuStrip();
            menu_file = new System.Windows.Forms.ToolStripMenuItem();
            menu_file_open = new System.Windows.Forms.ToolStripMenuItem();
            menu_file_openFolderKr = new System.Windows.Forms.ToolStripMenuItem();
            menu_file_openFolderRc = new System.Windows.Forms.ToolStripMenuItem();
            menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            menu_file_exit = new System.Windows.Forms.ToolStripMenuItem();
            menu_extract = new System.Windows.Forms.ToolStripMenuItem();
            menu_extract_all = new System.Windows.Forms.ToolStripMenuItem();
            menu_extract_current = new System.Windows.Forms.ToolStripMenuItem();
            menu_about = new System.Windows.Forms.ToolStripMenuItem();
            menu_lang = new System.Windows.Forms.ToolStripMenuItem();
            menu_debug = new System.Windows.Forms.ToolStripMenuItem();
            _listviewMain = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            _dialogSingleFile = new System.Windows.Forms.OpenFileDialog();
            _dialogMultiFile = new System.Windows.Forms.OpenFileDialog();
            imageList_listview = new System.Windows.Forms.ImageList(components);
            contextMenu_list = new System.Windows.Forms.ContextMenuStrip(components);
            fileMenuExtractfile = new System.Windows.Forms.ToolStripMenuItem();
            fileMenuExtractSelected = new System.Windows.Forms.ToolStripMenuItem();
            fileMenuConvertPNG = new System.Windows.Forms.ToolStripMenuItem();
            fileMenuConvertXML = new System.Windows.Forms.ToolStripMenuItem();
            _treeViewExplorer = new RhoLoader.Controls.DarkTreeView();
            split_main = new System.Windows.Forms.SplitContainer();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            _previewPanel = new RhoLoader.Controls.PreviewPanel.PreviewPanel();
            panel_topbar = new System.Windows.Forms.Panel();
            textbox_path = new System.Windows.Forms.TextBox();
            _iconBack = new System.Windows.Forms.PictureBox();
            _dialogKartData = new System.Windows.Forms.FolderBrowserDialog();
            _dialogExtractSelector = new System.Windows.Forms.FolderBrowserDialog();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            menu.SuspendLayout();
            contextMenu_list.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)split_main).BeginInit();
            split_main.Panel1.SuspendLayout();
            split_main.Panel2.SuspendLayout();
            split_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel_topbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_iconBack).BeginInit();
            SuspendLayout();
            // 
            // menu
            // 
            menu.BackColor = System.Drawing.Color.White;
            menu.Font = new System.Drawing.Font("Segoe UI", 9F);
            menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menu_file, menu_extract, menu_about, menu_lang, menu_debug });
            menu.Location = new System.Drawing.Point(0, 0);
            menu.Name = "menu";
            menu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            menu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            menu.Size = new System.Drawing.Size(967, 24);
            menu.TabIndex = 2;
            menu.Tag = "menu";
            menu.Text = "menu";
            // 
            // menu_file
            // 
            menu_file.BackColor = System.Drawing.Color.White;
            menu_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menu_file_open, menu_file_openFolderKr, menu_file_openFolderRc, menuToolStripMenuItem, toolStripSeparator1, menu_file_exit });
            menu_file.ForeColor = System.Drawing.Color.Black;
            menu_file.Name = "menu_file";
            menu_file.Size = new System.Drawing.Size(71, 20);
            menu_file.Tag = "menu_file";
            menu_file.Text = "menu_file";
            // 
            // menu_file_open
            // 
            menu_file_open.ForeColor = System.Drawing.Color.Black;
            menu_file_open.Name = "menu_file_open";
            menu_file_open.Size = new System.Drawing.Size(183, 22);
            menu_file_open.Tag = "menu_open";
            menu_file_open.Text = "menu_open";
            menu_file_open.Click += ActionOpen;
            // 
            // menu_file_openFolderKr
            // 
            menu_file_openFolderKr.ForeColor = System.Drawing.Color.Black;
            menu_file_openFolderKr.Name = "menu_file_openFolderKr";
            menu_file_openFolderKr.Size = new System.Drawing.Size(183, 22);
            menu_file_openFolderKr.Tag = "menu_openFolderKr";
            menu_file_openFolderKr.Text = "menu_openFolderKr";
            menu_file_openFolderKr.Click += ActionOpenFolderKr;
            // 
            // menu_file_openFolderRc
            // 
            menu_file_openFolderRc.ForeColor = System.Drawing.Color.Black;
            menu_file_openFolderRc.Name = "menu_file_openFolderRc";
            menu_file_openFolderRc.Size = new System.Drawing.Size(183, 22);
            menu_file_openFolderRc.Tag = "menu_openFolderRc";
            menu_file_openFolderRc.Text = "menu_openFolderRc";
            menu_file_openFolderRc.Click += ActionOpenFolderRc;
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            menuToolStripMenuItem.Text = "menu_save";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.ForeColor = System.Drawing.Color.Black;
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // menu_file_exit
            // 
            menu_file_exit.ForeColor = System.Drawing.Color.Black;
            menu_file_exit.Name = "menu_file_exit";
            menu_file_exit.Size = new System.Drawing.Size(183, 22);
            menu_file_exit.Tag = "menu_exit";
            menu_file_exit.Text = "menu_exit";
            menu_file_exit.Click += ActionExit;
            // 
            // menu_extract
            // 
            menu_extract.BackColor = System.Drawing.Color.White;
            menu_extract.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menu_extract_all, menu_extract_current });
            menu_extract.ForeColor = System.Drawing.Color.Black;
            menu_extract.Name = "menu_extract";
            menu_extract.Size = new System.Drawing.Size(90, 20);
            menu_extract.Tag = "menu_extract";
            menu_extract.Text = "menu_extract";
            // 
            // menu_extract_all
            // 
            menu_extract_all.ForeColor = System.Drawing.Color.Black;
            menu_extract_all.Name = "menu_extract_all";
            menu_extract_all.Size = new System.Drawing.Size(188, 22);
            menu_extract_all.Tag = "menu_extract_all";
            menu_extract_all.Text = "menu_extract_all";
            menu_extract_all.Click += ActionExtractAll;
            // 
            // menu_extract_current
            // 
            menu_extract_current.ForeColor = System.Drawing.Color.Black;
            menu_extract_current.Name = "menu_extract_current";
            menu_extract_current.Size = new System.Drawing.Size(188, 22);
            menu_extract_current.Tag = "menu_extract_current";
            menu_extract_current.Text = "menu_extract_current";
            menu_extract_current.Click += ActionExtractCurrent;
            // 
            // menu_about
            // 
            menu_about.BackColor = System.Drawing.Color.White;
            menu_about.ForeColor = System.Drawing.Color.Black;
            menu_about.Name = "menu_about";
            menu_about.Size = new System.Drawing.Size(86, 20);
            menu_about.Tag = "menu_about";
            menu_about.Text = "menu_about";
            menu_about.Click += ActionAboutWindow;
            // 
            // menu_lang
            // 
            menu_lang.BackColor = System.Drawing.Color.White;
            menu_lang.ForeColor = System.Drawing.Color.Black;
            menu_lang.Name = "menu_lang";
            menu_lang.Size = new System.Drawing.Size(112, 20);
            menu_lang.Tag = "menu_Languages";
            menu_lang.Text = "menu_Languages";
            // 
            // menu_debug
            // 
            menu_debug.BackColor = System.Drawing.Color.White;
            menu_debug.ForeColor = System.Drawing.Color.Black;
            menu_debug.Name = "menu_debug";
            menu_debug.Size = new System.Drawing.Size(54, 20);
            menu_debug.Tag = "menu_debug";
            menu_debug.Text = "Debug";
            menu_debug.Click += ActionDebug;
            // 
            // _listviewMain
            // 
            _listviewMain.AllowDrop = true;
            _listviewMain.BackColor = System.Drawing.Color.White;
            _listviewMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _listviewMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader3, columnHeader2 });
            _listviewMain.Dock = System.Windows.Forms.DockStyle.Fill;
            _listviewMain.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F);
            _listviewMain.FullRowSelect = true;
            _listviewMain.Location = new System.Drawing.Point(0, 0);
            _listviewMain.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            _listviewMain.Name = "_listviewMain";
            _listviewMain.Size = new System.Drawing.Size(469, 478);
            _listviewMain.TabIndex = 5;
            _listviewMain.UseCompatibleStateImageBehavior = false;
            _listviewMain.View = System.Windows.Forms.View.Details;
            _listviewMain.SelectedIndexChanged += ActionSelectItemChanged;
            _listviewMain.MouseClick += ActionListViewClick;
            _listviewMain.MouseDoubleClick += ActionListviewDoubleclick;
            // 
            // columnHeader1
            // 
            columnHeader1.Name = "columnHeader1";
            columnHeader1.Tag = "col_name";
            columnHeader1.Text = "col_name";
            columnHeader1.Width = 104;
            // 
            // columnHeader3
            // 
            columnHeader3.Name = "columnHeader3";
            columnHeader3.Tag = "col_type";
            columnHeader3.Text = "col_type";
            columnHeader3.Width = 125;
            // 
            // columnHeader2
            // 
            columnHeader2.Name = "columnHeader2";
            columnHeader2.Tag = "col_size";
            columnHeader2.Text = "col_size";
            columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeader2.Width = 75;
            // 
            // _dialogSingleFile
            // 
            _dialogSingleFile.Filter = "Rho and Jmd File | *.rho;*.jmd";
            // 
            // _dialogMultiFile
            // 
            _dialogMultiFile.Filter = "Rho or Jmd File|*.rho;*.jmd";
            _dialogMultiFile.Multiselect = true;
            // 
            // imageList_listview
            // 
            imageList_listview.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList_listview.ImageSize = new System.Drawing.Size(16, 16);
            imageList_listview.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // contextMenu_list
            // 
            contextMenu_list.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileMenuExtractfile, fileMenuExtractSelected, fileMenuConvertPNG, fileMenuConvertXML });
            contextMenu_list.Name = "FileMenu";
            contextMenu_list.Size = new System.Drawing.Size(210, 92);
            // 
            // fileMenuExtractfile
            // 
            fileMenuExtractfile.Name = "fileMenuExtractfile";
            fileMenuExtractfile.Size = new System.Drawing.Size(209, 22);
            fileMenuExtractfile.Tag = "filemenu_extractfile";
            fileMenuExtractfile.Text = "filemenu_extractfile";
            fileMenuExtractfile.Click += ActionExtractfile;
            // 
            // fileMenuExtractSelected
            // 
            fileMenuExtractSelected.Name = "fileMenuExtractSelected";
            fileMenuExtractSelected.Size = new System.Drawing.Size(209, 22);
            fileMenuExtractSelected.Tag = "filemenu_extract_selected";
            fileMenuExtractSelected.Text = "filemenu_extract_selected";
            fileMenuExtractSelected.Click += ActionExtractSelected;
            // 
            // fileMenuConvertPNG
            // 
            fileMenuConvertPNG.Name = "fileMenuConvertPNG";
            fileMenuConvertPNG.Size = new System.Drawing.Size(209, 22);
            fileMenuConvertPNG.Tag = "filemenu_convertPng";
            fileMenuConvertPNG.Text = "filemenu_convertPng";
            fileMenuConvertPNG.Click += ActionConvertPng;
            // 
            // fileMenuConvertXML
            // 
            fileMenuConvertXML.Name = "fileMenuConvertXML";
            fileMenuConvertXML.Size = new System.Drawing.Size(209, 22);
            fileMenuConvertXML.Tag = "filemenu_convertXML";
            fileMenuConvertXML.Text = "filemenu_convertXML";
            fileMenuConvertXML.Click += ActionConvertXml;
            // 
            // _treeViewExplorer
            // 
            _treeViewExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            _treeViewExplorer.FullRowSelect = true;
            _treeViewExplorer.ItemHeight = 20;
            _treeViewExplorer.Location = new System.Drawing.Point(0, 0);
            _treeViewExplorer.Name = "_treeViewExplorer";
            _treeViewExplorer.ShowPlusMinus = false;
            _treeViewExplorer.Size = new System.Drawing.Size(250, 478);
            _treeViewExplorer.TabIndex = 0;
            _treeViewExplorer.Tag = "treeview_explorer";
            _treeViewExplorer.AfterSelect += ActionNodeSelect;
            // 
            // split_main
            // 
            split_main.Dock = System.Windows.Forms.DockStyle.Fill;
            split_main.Location = new System.Drawing.Point(0, 48);
            split_main.Name = "split_main";
            // 
            // split_main.Panel1
            // 
            split_main.Panel1.Controls.Add(_treeViewExplorer);
            split_main.Panel1MinSize = 250;
            // 
            // split_main.Panel2
            // 
            split_main.Panel2.Controls.Add(splitContainer1);
            split_main.Panel2MinSize = 315;
            split_main.Size = new System.Drawing.Size(967, 478);
            split_main.SplitterDistance = 250;
            split_main.SplitterWidth = 2;
            split_main.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(_listviewMain);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            splitContainer1.Panel2.Controls.Add(_previewPanel);
            splitContainer1.Size = new System.Drawing.Size(715, 478);
            splitContainer1.SplitterDistance = 469;
            splitContainer1.TabIndex = 6;
            // 
            // _previewPanel
            // 
            _previewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _previewPanel.Location = new System.Drawing.Point(0, 0);
            _previewPanel.Name = "_previewPanel";
            _previewPanel.Size = new System.Drawing.Size(242, 478);
            _previewPanel.TabIndex = 0;
            _previewPanel.CreateControl += ActionCreatePreviewControl;
            _previewPanel.InitControl += ActionInitPreviewControl;
            // 
            // panel_topbar
            // 
            panel_topbar.Controls.Add(textbox_path);
            panel_topbar.Controls.Add(_iconBack);
            panel_topbar.Dock = System.Windows.Forms.DockStyle.Top;
            panel_topbar.Location = new System.Drawing.Point(0, 24);
            panel_topbar.Name = "panel_topbar";
            panel_topbar.Size = new System.Drawing.Size(967, 24);
            panel_topbar.TabIndex = 7;
            // 
            // textbox_path
            // 
            textbox_path.BackColor = System.Drawing.Color.White;
            textbox_path.Dock = System.Windows.Forms.DockStyle.Fill;
            textbox_path.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9F, System.Drawing.FontStyle.Bold);
            textbox_path.Location = new System.Drawing.Point(24, 0);
            textbox_path.Margin = new System.Windows.Forms.Padding(0);
            textbox_path.Name = "textbox_path";
            textbox_path.ReadOnly = true;
            textbox_path.Size = new System.Drawing.Size(943, 23);
            textbox_path.TabIndex = 0;
            // 
            // _iconBack
            // 
            _iconBack.Dock = System.Windows.Forms.DockStyle.Left;
            _iconBack.Enabled = false;
            _iconBack.Image = global::RhoLoader.Properties.Resources.ic_fluent_arrow_hook_up_left_24_filled_disabled;
            _iconBack.Location = new System.Drawing.Point(0, 0);
            _iconBack.Name = "_iconBack";
            _iconBack.Size = new System.Drawing.Size(24, 24);
            _iconBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _iconBack.TabIndex = 1;
            _iconBack.TabStop = false;
            _iconBack.EnabledChanged += ActionIconEnableChanged;
            _iconBack.Click += ActionBack;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(32, 19);
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(967, 526);
            Controls.Add(split_main);
            Controls.Add(panel_topbar);
            Controls.Add(menu);
            MainMenuStrip = menu;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tag = "title";
            Text = "title";
            menu.ResumeLayout(false);
            menu.PerformLayout();
            contextMenu_list.ResumeLayout(false);
            split_main.Panel1.ResumeLayout(false);
            split_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)split_main).EndInit();
            split_main.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel_topbar.ResumeLayout(false);
            panel_topbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_iconBack).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

        private System.Windows.Forms.SplitContainer splitContainer1;

        private System.Windows.Forms.FolderBrowserDialog _dialogExtractSelector;

        private System.Windows.Forms.FolderBrowserDialog _dialogKartData;

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem menu_file;
        private System.Windows.Forms.ToolStripMenuItem menu_file_open;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menu_file_exit;
        private System.Windows.Forms.ListView _listviewMain;
        private System.Windows.Forms.OpenFileDialog _dialogSingleFile;
        private System.Windows.Forms.OpenFileDialog _dialogMultiFile;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStripMenuItem menu_about;
        private System.Windows.Forms.ImageList imageList_listview;
        private System.Windows.Forms.ToolStripMenuItem menu_extract;
        private System.Windows.Forms.ToolStripMenuItem menu_extract_all;
        private System.Windows.Forms.ToolStripMenuItem menu_lang;
        private System.Windows.Forms.ToolStripMenuItem menu_debug;
        private System.Windows.Forms.ToolStripMenuItem menu_extract_current;
        private System.Windows.Forms.ContextMenuStrip contextMenu_list;
        private System.Windows.Forms.ToolStripMenuItem fileMenuExtractfile;
        private System.Windows.Forms.ToolStripMenuItem fileMenuConvertPNG;
        private System.Windows.Forms.ToolStripMenuItem fileMenuConvertXML;
        private System.Windows.Forms.ToolStripMenuItem fileMenuExtractSelected;
        private System.Windows.Forms.ToolStripMenuItem menu_file_openFolderKr;
        private System.Windows.Forms.ToolStripMenuItem menu_file_openFolderRc;
        private System.Windows.Forms.SplitContainer split_main;
        private System.Windows.Forms.Panel panel_topbar;
        private System.Windows.Forms.TextBox textbox_path;
        private RhoLoader.Controls.PreviewPanel.PreviewPanel _previewPanel;
        private PictureBox _iconBack;
        private RhoLoader.Controls.DarkTreeView _treeViewExplorer;
        private ToolStripMenuItem menuToolStripMenuItem;
    }
}

