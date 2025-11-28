using System.ComponentModel;

namespace RhoLoader;

partial class LoadingDialog
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
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
        label1.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
        label1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
        label1.Location = new System.Drawing.Point(0, 0);
        label1.Name = "label1";
        label1.Padding = new System.Windows.Forms.Padding(25, 0, 0, 0);
        label1.Size = new System.Drawing.Size(478, 70);
        label1.TabIndex = 0;
        label1.Text = "Loading";
        label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label2
        // 
        label2.Font = new System.Drawing.Font("Segoe UI Variable Small Semibol", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
        label2.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
        label2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
        label2.Location = new System.Drawing.Point(6, 56);
        label2.Name = "label2";
        label2.Padding = new System.Windows.Forms.Padding(25, 0, 0, 0);
        label2.Size = new System.Drawing.Size(478, 30);
        label2.TabIndex = 1;
        label2.Text = "Please wait...";
        label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // LoadingDialog
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.FromArgb(((int)((byte)32)), ((int)((byte)32)), ((int)((byte)32)));
        ClientSize = new System.Drawing.Size(396, 104);
        Controls.Add(label2);
        Controls.Add(label1);
        ForeColor = System.Drawing.Color.Black;
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        Opacity = 0.01D;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Loading";
        ResumeLayout(false);
    }

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.Label label1;

    #endregion
}