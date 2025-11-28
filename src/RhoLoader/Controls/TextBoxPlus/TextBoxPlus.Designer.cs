using System.ComponentModel;

namespace RhoLoader.Controls.TextBoxPlus;

partial class TextBoxPlus
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _vScrollBar = new System.Windows.Forms.VScrollBar();
        _hScrollBar = new System.Windows.Forms.HScrollBar();
        _cursorFlicker = new System.Windows.Forms.Timer(components);
        _contextMenu = new System.Windows.Forms.ContextMenuStrip(components);
        _contextMenuCopyItem = new System.Windows.Forms.ToolStripMenuItem();
        _contextMenuSelectAllItem = new System.Windows.Forms.ToolStripMenuItem();
        _contextMenu.SuspendLayout();
        SuspendLayout();
        // 
        // _vScrollBar
        // 
        _vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
        _vScrollBar.LargeChange = 5;
        _vScrollBar.Location = new System.Drawing.Point(382, 0);
        _vScrollBar.Name = "_vScrollBar";
        _vScrollBar.Size = new System.Drawing.Size(17, 367);
        _vScrollBar.TabIndex = 0;
        _vScrollBar.Scroll += ActionVScroll;
        _vScrollBar.ValueChanged += ActionVScrollValueChanged;
        _vScrollBar.PreviewKeyDown += ActionPreviewKeyDown;
        _vScrollBar.KeyDown += ActionKeyDown;
        // 
        // _hScrollBar
        // 
        _hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
        _hScrollBar.Location = new System.Drawing.Point(0, 350);
        _hScrollBar.Visible = false;
        _hScrollBar.Value = 0;
        _hScrollBar.Name = "_hScrollBar";
        _hScrollBar.Size = new System.Drawing.Size(382, 17);
        _hScrollBar.TabIndex = 1;
        _hScrollBar.SmallChange = 1;
        _hScrollBar.LargeChange = 1;
        _hScrollBar.ValueChanged += ActionHScrollValueChanged;
        _hScrollBar.PreviewKeyDown += ActionPreviewKeyDown;
        _hScrollBar.KeyDown += ActionKeyDown;
        // 
        // _cursorFlicker
        // 
        _cursorFlicker.Interval = 500;
        _cursorFlicker.Tick += ActionCursorFlick;
        // 
        // _contextMenu
        // 
        _contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _contextMenuCopyItem, _contextMenuSelectAllItem });
        _contextMenu.Name = "_contextMenu";
        _contextMenu.Size = new System.Drawing.Size(181, 70);
        // 
        // _contextMenuCopyItem
        // 
        _contextMenuCopyItem.Name = "_contextMenuCopyItem";
        _contextMenuCopyItem.Size = new System.Drawing.Size(39, 19);
        _contextMenuCopyItem.Text = "Copy";
        _contextMenuCopyItem.Click += ActionCopyClick;
        // 
        // _contextMenuSelectAllItem
        // 
        _contextMenuSelectAllItem.Name = "_contextMenuSelectAllItem";
        _contextMenuSelectAllItem.Size = new System.Drawing.Size(57, 19);
        _contextMenuSelectAllItem.Text = "Select all";
        _contextMenuSelectAllItem.Click += ActionSelectAllClick;
        // 
        // TextBoxPlus
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.SystemColors.Control;
        Controls.Add(_hScrollBar);
        Controls.Add(_vScrollBar);
        DoubleBuffered = true;
        Location = new System.Drawing.Point(15, 15);
        Size = new System.Drawing.Size(399, 367);
        Load += ActionLoad;
        Paint += ActionPaint;
        KeyDown += ActionKeyDown;
        KeyPress += ActionKeyPress;
        MouseClick += ActionMouseClick;
        MouseDown += ActionMouseDown;
        MouseMove += ActionMouseMove;
        Resize += ActionResize;
        _contextMenu.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.ContextMenuStrip _contextMenu;

    private System.Windows.Forms.ToolStripMenuItem _contextMenuCopyItem;
    
    private System.Windows.Forms.ToolStripMenuItem _contextMenuSelectAllItem;

    private System.Windows.Forms.Timer _cursorFlicker;

    private System.Windows.Forms.HScrollBar _hScrollBar;

    private System.Windows.Forms.VScrollBar _vScrollBar;
   #endregion
}