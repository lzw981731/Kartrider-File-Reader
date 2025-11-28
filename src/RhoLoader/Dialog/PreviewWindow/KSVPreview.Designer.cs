namespace RhoLoader.PreviewWindow
{
    partial class KsvPreview
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
            contestName = new System.Windows.Forms.Label();
            infoBox = new System.Windows.Forms.ListView();
            key = new System.Windows.Forms.ColumnHeader();
            value = new System.Windows.Forms.ColumnHeader();
            players = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            panel1 = new System.Windows.Forms.Panel();
            label2 = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // contestName
            // 
            contestName.BackColor = System.Drawing.Color.FromArgb(((int)((byte)48)), ((int)((byte)48)), ((int)((byte)48)));
            contestName.Dock = System.Windows.Forms.DockStyle.Top;
            contestName.Font = new System.Drawing.Font("Meiryo UI", 15.75F);
            contestName.ForeColor = System.Drawing.Color.White;
            contestName.Location = new System.Drawing.Point(0, 0);
            contestName.Margin = new System.Windows.Forms.Padding(0);
            contestName.Name = "contestName";
            contestName.Size = new System.Drawing.Size(934, 41);
            contestName.TabIndex = 0;
            contestName.Text = "這是個測試用文字 这是个测试用文字 이것은 테스트 텍스트입니다";
            contestName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoBox
            // 
            infoBox.BackColor = System.Drawing.Color.FromArgb(((int)((byte)36)), ((int)((byte)36)), ((int)((byte)36)));
            infoBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            infoBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { key, value });
            infoBox.Dock = System.Windows.Forms.DockStyle.Fill;
            infoBox.Font = new System.Drawing.Font("Meiryo", 9.75F);
            infoBox.ForeColor = System.Drawing.Color.White;
            infoBox.FullRowSelect = true;
            infoBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            infoBox.Location = new System.Drawing.Point(0, 41);
            infoBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            infoBox.MultiSelect = false;
            infoBox.Name = "infoBox";
            infoBox.Size = new System.Drawing.Size(934, 478);
            infoBox.TabIndex = 1;
            infoBox.UseCompatibleStateImageBehavior = false;
            infoBox.View = System.Windows.Forms.View.Details;
            // 
            // key
            // 
            key.Name = "key";
            key.Width = 215;
            // 
            // value
            // 
            value.Name = "value";
            value.Width = 215;
            // 
            // players
            // 
            players.Activation = System.Windows.Forms.ItemActivation.OneClick;
            players.BorderStyle = System.Windows.Forms.BorderStyle.None;
            players.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            players.Dock = System.Windows.Forms.DockStyle.Fill;
            players.Font = new System.Drawing.Font("Meiryo", 9.75F);
            players.FullRowSelect = true;
            players.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            players.Location = new System.Drawing.Point(0, 30);
            players.Margin = new System.Windows.Forms.Padding(0);
            players.Name = "players";
            players.Size = new System.Drawing.Size(433, 448);
            players.TabIndex = 2;
            players.UseCompatibleStateImageBehavior = false;
            players.View = System.Windows.Forms.View.Details;
            players.ItemSelectionChanged += players_ItemSelectionChanged;
            players.MouseHover += players_MouseHover;
            // 
            // columnHeader1
            // 
            columnHeader1.Name = "columnHeader1";
            columnHeader1.Width = 430;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.IndianRed;
            panel1.Controls.Add(players);
            panel1.Controls.Add(label2);
            panel1.Dock = System.Windows.Forms.DockStyle.Right;
            panel1.Location = new System.Drawing.Point(501, 41);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(433, 478);
            panel1.TabIndex = 3;
            // 
            // label2
            // 
            label2.BackColor = System.Drawing.Color.FromArgb(((int)((byte)235)), ((int)((byte)235)), ((int)((byte)235)));
            label2.Dock = System.Windows.Forms.DockStyle.Top;
            label2.Font = new System.Drawing.Font("Bahnschrift", 12F);
            label2.ForeColor = System.Drawing.Color.Black;
            label2.Location = new System.Drawing.Point(0, 0);
            label2.Margin = new System.Windows.Forms.Padding(0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(433, 30);
            label2.TabIndex = 5;
            label2.Text = "Players";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // KSVPreview
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(934, 519);
            Controls.Add(panel1);
            Controls.Add(infoBox);
            Controls.Add(contestName);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Text = "KSVPreview";
            Load += KSVPreview_Load;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label contestName;
        private System.Windows.Forms.ListView infoBox;
        private System.Windows.Forms.ColumnHeader key;
        private System.Windows.Forms.ColumnHeader value;
        private System.Windows.Forms.ListView players;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private Panel panel1;
        private Label label2;
    }
}