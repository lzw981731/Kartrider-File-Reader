using System.ComponentModel;

namespace RhoLoader.Dialog;

partial class TestWindow
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        _textEditPanel = new System.Windows.Forms.Panel();
        _testTab = new System.Windows.Forms.TabControl();
        _textBoxPlusTextPanel = new System.Windows.Forms.TabPage();
        _textBoxPlus = new RhoLoader.Controls.TextBoxPlus.TextBoxPlus();
        _musicPreviewTestPanel = new System.Windows.Forms.TabPage();
        _pictureViewerTestPanel = new System.Windows.Forms.TabPage();
        _pictureViewerWpf = new System.Windows.Forms.TabPage();
        _textEditPanel.SuspendLayout();
        _testTab.SuspendLayout();
        _textBoxPlusTextPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _textEditPanel
        // 
        _textEditPanel.Controls.Add(_testTab);
        _textEditPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        _textEditPanel.Location = new System.Drawing.Point(0, 0);
        _textEditPanel.Name = "_textEditPanel";
        _textEditPanel.Size = new System.Drawing.Size(1193, 701);
        _textEditPanel.TabIndex = 0;
        // 
        // _testTab
        // 
        _testTab.Controls.Add(_textBoxPlusTextPanel);
        _testTab.Controls.Add(_musicPreviewTestPanel);
        _testTab.Controls.Add(_pictureViewerTestPanel);
        _testTab.Controls.Add(_pictureViewerWpf);
        _testTab.Dock = System.Windows.Forms.DockStyle.Fill;
        _testTab.Location = new System.Drawing.Point(0, 0);
        _testTab.Name = "_testTab";
        _testTab.SelectedIndex = 0;
        _testTab.Size = new System.Drawing.Size(1193, 701);
        _testTab.TabIndex = 2;
        // 
        // _textBoxPlusTextPanel
        // 
        _textBoxPlusTextPanel.Controls.Add(_textBoxPlus);
        _textBoxPlusTextPanel.Location = new System.Drawing.Point(4, 24);
        _textBoxPlusTextPanel.Name = "_textBoxPlusTextPanel";
        _textBoxPlusTextPanel.Padding = new System.Windows.Forms.Padding(3);
        _textBoxPlusTextPanel.Size = new System.Drawing.Size(1185, 673);
        _textBoxPlusTextPanel.TabIndex = 0;
        _textBoxPlusTextPanel.Text = "TextboxPlus";
        _textBoxPlusTextPanel.UseVisualStyleBackColor = true;
        // 
        // _textBoxPlus
        // 
        _textBoxPlus.BackColor = System.Drawing.Color.White;
        _textBoxPlus.Dock = System.Windows.Forms.DockStyle.Fill;
        _textBoxPlus.Font = new System.Drawing.Font("Red Hat Mono Medium", 12F);
        _textBoxPlus.Location = new System.Drawing.Point(3, 3);
        _textBoxPlus.Name = "_textBoxPlus";
        _textBoxPlus.Size = new System.Drawing.Size(1179, 667);
        _textBoxPlus.TabIndex = 0;
        // 
        // _musicPreviewTestPanel
        // 
        _musicPreviewTestPanel.Location = new System.Drawing.Point(4, 24);
        _musicPreviewTestPanel.Name = "_musicPreviewTestPanel";
        _musicPreviewTestPanel.Padding = new System.Windows.Forms.Padding(3);
        _musicPreviewTestPanel.Size = new System.Drawing.Size(1185, 673);
        _musicPreviewTestPanel.TabIndex = 1;
        _musicPreviewTestPanel.Text = "MusicPlayer";
        _musicPreviewTestPanel.UseVisualStyleBackColor = true;
        // 
        // _pictureViewerTestPanel
        // 
        _pictureViewerTestPanel.Location = new System.Drawing.Point(4, 24);
        _pictureViewerTestPanel.Name = "_pictureViewerTestPanel";
        _pictureViewerTestPanel.Padding = new System.Windows.Forms.Padding(3);
        _pictureViewerTestPanel.Size = new System.Drawing.Size(1185, 673);
        _pictureViewerTestPanel.TabIndex = 2;
        _pictureViewerTestPanel.Text = "PictureViewer";
        _pictureViewerTestPanel.UseVisualStyleBackColor = true;
        // 
        // _pictureViewerWpf
        // 
        _pictureViewerWpf.Location = new System.Drawing.Point(4, 24);
        _pictureViewerWpf.Name = "_pictureViewerWpf";
        _pictureViewerWpf.Padding = new System.Windows.Forms.Padding(3);
        _pictureViewerWpf.Size = new System.Drawing.Size(1185, 673);
        _pictureViewerWpf.TabIndex = 3;
        _pictureViewerWpf.Text = "PictureViewerWpf";
        _pictureViewerWpf.UseVisualStyleBackColor = true;
        // 
        // TestWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1193, 701);
        Controls.Add(_textEditPanel);
        Text = "TestWindow";
        Load += ActionLoad;
        _textEditPanel.ResumeLayout(false);
        _testTab.ResumeLayout(false);
        _textBoxPlusTextPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.TabPage _pictureViewerWpf;

    private System.Windows.Forms.TabControl _testTab;
    private System.Windows.Forms.TabPage _textBoxPlusTextPanel;
    private System.Windows.Forms.TabPage _musicPreviewTestPanel;
    private System.Windows.Forms.TabPage _pictureViewerTestPanel;

    private RhoLoader.Controls.TextBoxPlus.TextBoxPlus _textBoxPlus;

    private System.Windows.Forms.Panel _textEditPanel;

    #endregion
}