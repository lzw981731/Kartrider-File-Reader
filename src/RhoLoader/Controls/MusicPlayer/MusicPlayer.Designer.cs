using System.ComponentModel;

namespace RhoLoader.Controls.MusicPlayer;

partial class MusicPlayer
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
        button1 = new System.Windows.Forms.Button();
        _updateTimer = new System.Windows.Forms.Timer(components);
        button2 = new System.Windows.Forms.Button();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Location = new System.Drawing.Point(23, 138);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(75, 23);
        button1.TabIndex = 0;
        button1.Text = "Play";
        button1.UseVisualStyleBackColor = true;
        button1.Click += ActionPlayPause;
        // 
        // _updateTimer
        // 
        _updateTimer.Enabled = true;
        _updateTimer.Interval = 1;
        _updateTimer.Tick += ActionUpdateTimer;
        // 
        // button2
        // 
        button2.Location = new System.Drawing.Point(208, 100);
        button2.Name = "button2";
        button2.Size = new System.Drawing.Size(75, 23);
        button2.TabIndex = 1;
        button2.Text = "button2";
        button2.UseVisualStyleBackColor = true;
        button2.Click += ActionDebug;
        // 
        // MusicPlayer
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.SystemColors.ActiveCaption;
        Controls.Add(button2);
        Controls.Add(button1);
        DoubleBuffered = true;
        Size = new System.Drawing.Size(434, 264);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button button2;

    private System.Windows.Forms.Timer _updateTimer;

    private System.Windows.Forms.Button button1;

    #endregion
}