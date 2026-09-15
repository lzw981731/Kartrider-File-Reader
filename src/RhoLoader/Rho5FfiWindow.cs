using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RhoLoader
{
    /// <summary>
    /// RHO5 (P5136) 资源工具窗体：基于 Rust FFI (rho5ffi.dll) 提供
    /// 打开 Data 目录 → 列出条目 → 提取 → 替换/新增 → 拖拽导入。
    /// </summary>
    public class Rho5FfiWindow : Form
    {
        private IntPtr _handle = IntPtr.Zero;
        private ListView _list;
        private TextBox _txtPath;
        private Button _btnOpen;
        private Button _btnExtract;
        private Button _btnReplace;
        private Label _lblStatus;

        public Rho5FfiWindow()
        {
            BuildUi();
            this.AllowDrop = true;
            this.DragEnter += OnDragEnter;
            this.DragDrop += OnDragDrop;
            this.FormClosed += (s, e) =>
            {
                if (_handle != IntPtr.Zero)
                {
                    Rho5Ffi.Close(_handle);
                    _handle = IntPtr.Zero;
                }
            };
        }

        private void BuildUi()
        {
            this.Text = "RHO5 Tool (P5136 / Rust FFI)";
            this.Size = new Size(760, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(8) };
            _txtPath = new TextBox
            {
                Location = new Point(8, 10),
                Width = 540,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                Text = "选择 P5136 客户端的 Data 目录"
            };
            _btnOpen = new Button { Text = "打开 Data 目录", Location = new Point(556, 8), Width = 90 };
            _btnOpen.Click += (s, e) => OpenDataDirectory();
            top.Controls.Add(_txtPath);
            top.Controls.Add(_btnOpen);
            this.Controls.Add(top);

            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                AllowDrop = true
            };
            _list.Columns.Add("内部路径", 380);
            _list.Columns.Add("归档", 160);
            _list.Columns.Add("压缩", 70);
            _list.Columns.Add("解压", 70);
            _list.DragEnter += OnDragEnter;
            _list.DragDrop += OnDragDrop;
            this.Controls.Add(_list);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(8) };
            _btnExtract = new Button { Text = "提取选中", Location = new Point(8, 12), Width = 90 };
            _btnExtract.Click += (s, e) => ExtractSelected();
            _btnReplace = new Button { Text = "替换选中(选文件)", Location = new Point(106, 12), Width = 130 };
            _btnReplace.Click += (s, e) => ReplaceSelectedWithFile();
            _lblStatus = new Label
            {
                Location = new Point(8, 48),
                AutoSize = false,
                Width = 720,
                Height = 20,
                Text = "就绪。拖拽任意文件到列表框 = 替换/新增条目",
                ForeColor = Color.Gray
            };
            bottom.Controls.Add(_btnExtract);
            bottom.Controls.Add(_btnReplace);
            bottom.Controls.Add(_lblStatus);
            this.Controls.Add(bottom);
        }

        private void SetStatus(string message, bool isError = false)
        {
            _lblStatus.Text = message;
            _lblStatus.ForeColor = isError ? Color.Red : Color.Gray;
        }

        private void OpenDataDirectory()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择 P5136 客户端的 Data 目录(包含 DataPack*.rho5)";
                fbd.UseDescriptionForTitle = true;
                if (fbd.ShowDialog(this) != DialogResult.OK)
                    return;
                string dataDir = fbd.SelectedPath;
                try
                {
                    if (_handle != IntPtr.Zero)
                    {
                        Rho5Ffi.Close(_handle);
                        _handle = IntPtr.Zero;
                    }
                    _handle = Rho5Ffi.Open(dataDir);
                    _txtPath.Text = dataDir;
                    RefreshList();
                    SetStatus("已打开: " + dataDir);
                }
                catch (Exception ex)
                {
                    SetStatus("打开失败: " + ex.Message, true);
                }
            }
        }

        private void RefreshList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            if (_handle == IntPtr.Zero)
            {
                _list.EndUpdate();
                return;
            }
            try
            {
                foreach (var entry in Rho5Ffi.List(_handle))
                {
                    var item = new ListViewItem(entry.Path) { Tag = entry };
                    item.SubItems.Add(entry.Archive);
                    item.SubItems.Add(entry.CompressedSize.ToString());
                    item.SubItems.Add(entry.PlaintextSize.ToString());
                    _list.Items.Add(item);
                }
                SetStatus($"共 {_list.Items.Count} 个条目");
            }
            catch (Exception ex)
            {
                SetStatus("列出失败: " + ex.Message, true);
            }
            finally
            {
                _list.EndUpdate();
            }
        }

        private void ExtractSelected()
        {
            if (_list.SelectedItems.Count == 0)
            {
                SetStatus("请先选中一个条目", true);
                return;
            }
            string path = _list.SelectedItems[0].Tag is Rho5Ffi.Entry e ? e.Path : "";
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = Path.GetFileName(path.Replace('/', '\\'));
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    byte[] data = Rho5Ffi.Extract(_handle, path);
                    File.WriteAllBytes(sfd.FileName, data);
                    SetStatus($"已提取 {path} -> {sfd.FileName} ({data.Length} bytes)");
                }
                catch (Exception ex)
                {
                    SetStatus("提取失败: " + ex.Message, true);
                }
            }
        }

        private void ReplaceSelectedWithFile()
        {
            if (_list.SelectedItems.Count == 0)
            {
                SetStatus("请先选中一个条目", true);
                return;
            }
            string targetPath = _list.SelectedItems[0].Tag is Rho5Ffi.Entry entry ? entry.Path : "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择用来替换的文件";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;
                ReplaceEntry(targetPath, ofd.FileName);
            }
        }

        private void ReplaceEntry(string targetPath, string filePath)
        {
            if (!File.Exists(filePath))
                return;
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                Rho5Ffi.Replace(_handle, targetPath, data);
                SetStatus($"已替换 {targetPath} <- {Path.GetFileName(filePath)} ({data.Length} bytes)，归档已写回(带.bak备份)");
            }
            catch (Exception ex)
            {
                SetStatus("替换失败: " + ex.Message, true);
            }
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            if (_handle == IntPtr.Zero)
            {
                SetStatus("请先打开 Data 目录", true);
                return;
            }
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string filePath = files[0];
                if (!File.Exists(filePath))
                    return;
                // 拖拽到选中条目 = 替换；无选中 = 新增到 etc_/ 前缀目标（保留文件名）
                string targetPath;
                if (_list.SelectedItems.Count > 0 && _list.SelectedItems[0].Tag is Rho5Ffi.Entry sel)
                    targetPath = sel.Path;
                else
                    targetPath = "etc_/" + Path.GetFileName(filePath);
                ReplaceEntry(targetPath, filePath);
                RefreshList();
            }
        }
    }
}