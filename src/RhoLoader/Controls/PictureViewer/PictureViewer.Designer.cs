using System.ComponentModel;

namespace RhoLoader.Controls.PictureViewer;

partial class PictureViewer
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
        _updateTimer = new System.Windows.Forms.Timer(components);
        SuspendLayout();
        // 
        // _updateTimer
        // 
        _updateTimer.Enabled = true;
        _updateTimer.Interval = 1;
        _updateTimer.Tick += ActionUpdateTimer;
        // 
        // PictureViewer
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        DoubleBuffered = true;
        Size = new System.Drawing.Size(426, 386);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Timer _updateTimer;

    #endregion
}