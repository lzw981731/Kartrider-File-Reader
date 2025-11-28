namespace RhoLoader
{
    partial class AboutMe
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
            label1 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            version = new System.Windows.Forms.Label();
            checkUpdateStatus = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
            label1.Location = new System.Drawing.Point(15, 10);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(211, 47);
            label1.TabIndex = 0;
            label1.Text = "KartCityLite";
            // 
            // button1
            // 
            button1.BackColor = System.Drawing.Color.White;
            button1.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
            button1.Location = new System.Drawing.Point(588, 15);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(131, 40);
            button1.TabIndex = 2;
            button1.Text = "Feedback";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
            label2.Location = new System.Drawing.Point(20, 70);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(610, 63);
            label2.TabIndex = 3;
            label2.Text = ("Rho Loader is a tool which helps you to explore the files included in Rho Type Fi" + "le. \r\nAuthor: eP Game Studio\r\nLicense: GPL v3");
            // 
            // version
            // 
            version.AutoSize = true;
            version.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
            version.Location = new System.Drawing.Point(255, 14);
            version.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            version.Name = "version";
            version.Size = new System.Drawing.Size(95, 21);
            version.TabIndex = 4;
            version.Text = "Version : 1.0";
            // 
            // checkUpdateStatus
            // 
            checkUpdateStatus.AutoSize = true;
            checkUpdateStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            checkUpdateStatus.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
            checkUpdateStatus.ForeColor = System.Drawing.Color.OrangeRed;
            checkUpdateStatus.Location = new System.Drawing.Point(257, 38);
            checkUpdateStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            checkUpdateStatus.Name = "checkUpdateStatus";
            checkUpdateStatus.Size = new System.Drawing.Size(236, 15);
            checkUpdateStatus.TabIndex = 5;
            checkUpdateStatus.Text = "New Version Released, Clicking for updating.";
            checkUpdateStatus.Click += checkUpdateStatus_Click;
            // 
            // AboutMe
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(741, 154);
            Controls.Add(checkUpdateStatus);
            Controls.Add(version);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "About";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label checkUpdateStatus;
    }
}