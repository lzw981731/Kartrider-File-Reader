namespace RhoLoader
{
    partial class ExtractFolder
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
            btn_cancel = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            statusText = new System.Windows.Forms.Label();
            progressMain = new RhoLoader.Controls.DarkProgressBar();
            label_extract = new System.Windows.Forms.Label();
            label_extracting = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label_progress = new System.Windows.Forms.Label();
            textExtractFile = new System.Windows.Forms.Label();
            textProgress = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btn_cancel
            // 
            btn_cancel.Font = new System.Drawing.Font("Segoe UI Variable Display", 9F);
            btn_cancel.ForeColor = System.Drawing.Color.Black;
            btn_cancel.Location = new System.Drawing.Point(481, 115);
            btn_cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(118, 34);
            btn_cancel.TabIndex = 4;
            btn_cancel.Text = "Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += actionCancel;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(69, 18);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(0, 15);
            label5.TabIndex = 6;
            // 
            // statusText
            // 
            statusText.AutoSize = true;
            statusText.Font = new System.Drawing.Font("Consolas", 9F);
            statusText.Location = new System.Drawing.Point(68, 18);
            statusText.Name = "statusText";
            statusText.Size = new System.Drawing.Size(0, 14);
            statusText.TabIndex = 8;
            // 
            // progressMain
            // 
            progressMain.BackColor = System.Drawing.Color.FromArgb(((int)((byte)225)), ((int)((byte)225)), ((int)((byte)225)));
            progressMain.Location = new System.Drawing.Point(-5, 97);
            progressMain.MaxValue = 1D;
            progressMain.Name = "progressMain";
            progressMain.Size = new System.Drawing.Size(617, 5);
            progressMain.TabIndex = 9;
            progressMain.Value = 0D;
            // 
            // label_extract
            // 
            label_extract.Font = new System.Drawing.Font("Segoe UI Variable Display", 24F);
            label_extract.Location = new System.Drawing.Point(0, 0);
            label_extract.Name = "label_extract";
            label_extract.Padding = new System.Windows.Forms.Padding(25, 0, 0, 0);
            label_extract.Size = new System.Drawing.Size(330, 70);
            label_extract.TabIndex = 10;
            label_extract.Text = "Extracting...";
            label_extract.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_extracting
            // 
            label_extracting.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9.75F, System.Drawing.FontStyle.Bold);
            label_extracting.Location = new System.Drawing.Point(18, 70);
            label_extracting.Name = "label_extracting";
            label_extracting.Size = new System.Drawing.Size(81, 23);
            label_extracting.TabIndex = 11;
            label_extracting.Text = "Extracting:";
            label_extracting.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 86);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(0, 15);
            label2.TabIndex = 12;
            // 
            // label_progress
            // 
            label_progress.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 9.75F, System.Drawing.FontStyle.Bold);
            label_progress.Location = new System.Drawing.Point(25, 105);
            label_progress.Name = "label_progress";
            label_progress.Size = new System.Drawing.Size(74, 23);
            label_progress.TabIndex = 13;
            label_progress.Text = "Progress:";
            label_progress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textExtractFile
            // 
            textExtractFile.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            textExtractFile.Location = new System.Drawing.Point(112, 70);
            textExtractFile.Name = "textExtractFile";
            textExtractFile.Size = new System.Drawing.Size(487, 23);
            textExtractFile.TabIndex = 14;
            textExtractFile.Text = "etc_/test.1s";
            textExtractFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textProgress
            // 
            textProgress.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            textProgress.Location = new System.Drawing.Point(112, 105);
            textProgress.Name = "textProgress";
            textProgress.Size = new System.Drawing.Size(109, 23);
            textProgress.TabIndex = 15;
            textProgress.Text = "3 / 256";
            textProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ExtractFolder
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(((int)((byte)32)), ((int)((byte)32)), ((int)((byte)32)));
            ClientSize = new System.Drawing.Size(612, 160);
            Controls.Add(textProgress);
            Controls.Add(textExtractFile);
            Controls.Add(label_progress);
            Controls.Add(label2);
            Controls.Add(label_extracting);
            Controls.Add(label_extract);
            Controls.Add(progressMain);
            Controls.Add(statusText);
            Controls.Add(label5);
            Controls.Add(btn_cancel);
            ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Text = "dialog_extract_folder";
            Shown += actionShow;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Label label5;
        private Label statusText;
        private RhoLoader.Controls.DarkProgressBar progressMain;
        private System.Windows.Forms.Label label_extract;
        private Label label_extracting;
        private Label label2;
        private Label label_progress;
        private Label textExtractFile;
        private Label textProgress;
    }
}