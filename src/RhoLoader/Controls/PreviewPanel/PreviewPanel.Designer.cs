using System.ComponentModel;

namespace RhoLoader.Controls.PreviewPanel;

partial class PreviewPanel
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
        _loadingPanel = new System.Windows.Forms.Panel();
        _previewPanel = new System.Windows.Forms.Panel();
        _prepareText = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // _loadingPanel
        // 
        _loadingPanel.BackColor = System.Drawing.Color.White;
        _loadingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        _loadingPanel.Location = new System.Drawing.Point(0, 0);
        _loadingPanel.Name = "_loadingPanel";
        _loadingPanel.Size = new System.Drawing.Size(279, 306);
        _loadingPanel.TabIndex = 0;
        // 
        // _previewPanel
        // 
        _previewPanel.BackColor = System.Drawing.Color.White;
        _previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        _previewPanel.Location = new System.Drawing.Point(0, 0);
        _previewPanel.Name = "_previewPanel";
        _previewPanel.Size = new System.Drawing.Size(279, 306);
        _previewPanel.TabIndex = 0;
        // 
        // _prepareText
        // 
        _prepareText.Dock = System.Windows.Forms.DockStyle.Fill;
        _prepareText.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 15F);
        _prepareText.Location = new System.Drawing.Point(0, 0);
        _prepareText.Name = "_prepareText";
        _prepareText.Size = new System.Drawing.Size(279, 306);
        _prepareText.TabIndex = 0;
        _prepareText.Text = "Prepare for previewing...";
        _prepareText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        _loadingPanel.Controls.Add(_prepareText);
        // 
        // PreviewPanel
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(_previewPanel);
        Controls.Add(_loadingPanel);
        Size = new System.Drawing.Size(279, 306);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Label _prepareText;

    private System.Windows.Forms.Panel _loadingPanel;
    private System.Windows.Forms.Panel _previewPanel;

    #endregion
}