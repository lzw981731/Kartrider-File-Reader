using System.ComponentModel;

namespace RhoLoader.Controls.PreviewPanel;

partial class XmlPreview
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
        _textBoxPlus = new RhoLoader.Controls.TextBoxPlus.TextBoxPlus();
        SuspendLayout();
        // 
        // textBoxPlus1
        // 
        _textBoxPlus.BackColor = System.Drawing.Color.White;
        _textBoxPlus.Dock = System.Windows.Forms.DockStyle.Fill;
        _textBoxPlus.Font = new System.Drawing.Font("Red Hat Mono Medium", 11F);
        _textBoxPlus.Location = new System.Drawing.Point(0, 0);
        _textBoxPlus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        _textBoxPlus.Name = "_textBoxPlus";
        _textBoxPlus.Size = new System.Drawing.Size(150, 150);
        _textBoxPlus.TabIndex = 0;
        // 
        // XmlPreview
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(_textBoxPlus);
        ResumeLayout(false);
    }

    private RhoLoader.Controls.TextBoxPlus.TextBoxPlus _textBoxPlus;

    #endregion
}