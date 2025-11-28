namespace RhoLoader
{
    partial class ExtractOption
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
            conBml = new System.Windows.Forms.CheckBox();
            conDds = new System.Windows.Forms.CheckBox();
            btn_extract = new System.Windows.Forms.Button();
            conTgs = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // conBml
            // 
            conBml.AutoSize = true;
            conBml.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
            conBml.Location = new System.Drawing.Point(12, 12);
            conBml.Name = "conBml";
            conBml.Size = new System.Drawing.Size(136, 19);
            conBml.TabIndex = 0;
            conBml.Text = "Convert BML to XML";
            conBml.UseVisualStyleBackColor = false;
            // 
            // conDds
            // 
            conDds.AutoSize = true;
            conDds.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
            conDds.Location = new System.Drawing.Point(12, 36);
            conDds.Name = "conDds";
            conDds.Size = new System.Drawing.Size(134, 19);
            conDds.TabIndex = 1;
            conDds.Text = "Convert DDS to PNG";
            conDds.UseVisualStyleBackColor = true;
            // 
            // btn_extract
            // 
            btn_extract.Location = new System.Drawing.Point(12, 84);
            btn_extract.Name = "btn_extract";
            btn_extract.Size = new System.Drawing.Size(133, 23);
            btn_extract.TabIndex = 2;
            btn_extract.Text = "Start Export";
            btn_extract.UseVisualStyleBackColor = true;
            btn_extract.Click += action_submit;
            // 
            // conTgs
            // 
            conTgs.AutoSize = true;
            conTgs.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
            conTgs.Location = new System.Drawing.Point(12, 60);
            conTgs.Name = "conTgs";
            conTgs.Size = new System.Drawing.Size(132, 19);
            conTgs.TabIndex = 3;
            conTgs.Text = "Convert TGS to PNG";
            conTgs.UseVisualStyleBackColor = true;
            // 
            // ExtractOption
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(((int)((byte)32)), ((int)((byte)32)), ((int)((byte)32)));
            ClientSize = new System.Drawing.Size(157, 118);
            Controls.Add(conTgs);
            Controls.Add(btn_extract);
            Controls.Add(conDds);
            Controls.Add(conBml);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Text = "Option";
            FormClosing += ExportOption_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.CheckBox conTgs;

        #endregion

        private System.Windows.Forms.CheckBox conBml;
        private System.Windows.Forms.CheckBox conDds;
        private System.Windows.Forms.Button btn_extract;
    }
}