namespace RhoLoader.PreviewWindow
{
    partial class ImageViewer
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
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            turnToDarkBackgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scale = new System.Windows.Forms.ToolStripMenuItem();
            poweredByPfiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            _pictureViewerPanel = new System.Windows.Forms.Panel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { turnToDarkBackgroundToolStripMenuItem, saveToPngToolStripMenuItem, scale, poweredByPfiToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            menuStrip1.Size = new System.Drawing.Size(933, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // turnToDarkBackgroundToolStripMenuItem
            // 
            turnToDarkBackgroundToolStripMenuItem.Name = "turnToDarkBackgroundToolStripMenuItem";
            turnToDarkBackgroundToolStripMenuItem.Size = new System.Drawing.Size(152, 20);
            turnToDarkBackgroundToolStripMenuItem.Text = "Turn to Dark Background";
            turnToDarkBackgroundToolStripMenuItem.Click += ActionTurnToDark;
            // 
            // saveToPngToolStripMenuItem
            // 
            saveToPngToolStripMenuItem.Name = "saveToPngToolStripMenuItem";
            saveToPngToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            saveToPngToolStripMenuItem.Text = "Save To Png";
            saveToPngToolStripMenuItem.Click += saveToPngToolStripMenuItem_Click;
            // 
            // scale
            // 
            scale.Enabled = false;
            scale.Name = "scale";
            scale.Size = new System.Drawing.Size(49, 20);
            scale.Text = "Scale:";
            // 
            // poweredByPfiToolStripMenuItem
            // 
            poweredByPfiToolStripMenuItem.Enabled = false;
            poweredByPfiToolStripMenuItem.Name = "poweredByPfiToolStripMenuItem";
            poweredByPfiToolStripMenuItem.Size = new System.Drawing.Size(109, 20);
            poweredByPfiToolStripMenuItem.Text = "Powered By Pfim";
            // 
            // _pictureViewerPanel
            // 
            _pictureViewerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _pictureViewerPanel.Location = new System.Drawing.Point(0, 24);
            _pictureViewerPanel.Name = "_pictureViewerPanel";
            _pictureViewerPanel.Size = new System.Drawing.Size(933, 495);
            _pictureViewerPanel.TabIndex = 0;
            // 
            // TgaDdsViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(933, 519);
            Controls.Add(_pictureViewerPanel);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Text = "Image Viewer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Panel _pictureViewerPanel;

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem turnToDarkBackgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scale;
        private System.Windows.Forms.ToolStripMenuItem poweredByPfiToolStripMenuItem;
    }
}