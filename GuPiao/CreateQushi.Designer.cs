namespace GuPiao
{
    partial class CreateQushi
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
            this.pnlButton = new System.Windows.Forms.Panel();
            this.chkNoChuangye = new System.Windows.Forms.CheckBox();
            this.lblConSel = new System.Windows.Forms.Label();
            this.cmbCon = new System.Windows.Forms.ComboBox();
            this.lblCntInfo = new System.Windows.Forms.Label();
            this.btnAft = new System.Windows.Forms.Button();
            this.btnBef = new System.Windows.Forms.Button();
            this.btnGetAllStock = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.imgBody = new System.Windows.Forms.PictureBox();
            this.pnlTopBody.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBody)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTopBody
            // 
            this.pnlTopBody.Controls.Add(this.pnlBody);
            this.pnlTopBody.Controls.Add(this.pnlButton);
            this.pnlTopBody.Size = new System.Drawing.Size(602, 362);
            // 
            // pnlButton
            // 
            this.pnlButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlButton.Controls.Add(this.chkNoChuangye);
            this.pnlButton.Controls.Add(this.lblConSel);
            this.pnlButton.Controls.Add(this.cmbCon);
            this.pnlButton.Controls.Add(this.lblCntInfo);
            this.pnlButton.Controls.Add(this.btnAft);
            this.pnlButton.Controls.Add(this.btnBef);
            this.pnlButton.Controls.Add(this.btnGetAllStock);
            this.pnlButton.Controls.Add(this.btnCreate);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 322);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(602, 40);
            this.pnlButton.TabIndex = 0;
            // 
            // chkNoChuangye
            // 
            this.chkNoChuangye.AutoSize = true;
            this.chkNoChuangye.Checked = true;
            this.chkNoChuangye.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNoChuangye.Enabled = false;
            this.chkNoChuangye.Location = new System.Drawing.Point(369, 12);
            this.chkNoChuangye.Name = "chkNoChuangye";
            this.chkNoChuangye.Size = new System.Drawing.Size(72, 16);
            this.chkNoChuangye.TabIndex = 9;
            this.chkNoChuangye.Text = "不要创业";
            this.chkNoChuangye.UseVisualStyleBackColor = true;
            // 
            // lblConSel
            // 
            this.lblConSel.AutoSize = true;
            this.lblConSel.Location = new System.Drawing.Point(213, 14);
            this.lblConSel.Name = "lblConSel";
            this.lblConSel.Size = new System.Drawing.Size(53, 12);
            this.lblConSel.TabIndex = 8;
            this.lblConSel.Text = "条件过滤";
            // 
            // cmbCon
            // 
            this.cmbCon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCon.FormattingEnabled = true;
            this.cmbCon.Items.AddRange(new object[] {
            "显示所有",
            "查看买点",
            "下跌递减",
            "下跌转折",
            "下跌转折两天",
            "下跌转折三天",
            "上涨转折",
            "一路下跌",
            "一路上涨"});
            this.cmbCon.Location = new System.Drawing.Point(268, 10);
            this.cmbCon.Name = "cmbCon";
            this.cmbCon.Size = new System.Drawing.Size(95, 20);
            this.cmbCon.TabIndex = 7;
            // 
            // lblCntInfo
            // 
            this.lblCntInfo.AutoSize = true;
            this.lblCntInfo.Location = new System.Drawing.Point(537, 14);
            this.lblCntInfo.Name = "lblCntInfo";
            this.lblCntInfo.Size = new System.Drawing.Size(59, 12);
            this.lblCntInfo.TabIndex = 6;
            this.lblCntInfo.Text = "9999/9999";
            // 
            // btnAft
            // 
            this.btnAft.Location = new System.Drawing.Point(492, 6);
            this.btnAft.Name = "btnAft";
            this.btnAft.Size = new System.Drawing.Size(39, 28);
            this.btnAft.TabIndex = 4;
            this.btnAft.Text = "后";
            this.btnAft.UseVisualStyleBackColor = true;
            this.btnAft.Click += new System.EventHandler(this.btnAft_Click);
            // 
            // btnBef
            // 
            this.btnBef.Location = new System.Drawing.Point(447, 6);
            this.btnBef.Name = "btnBef";
            this.btnBef.Size = new System.Drawing.Size(39, 28);
            this.btnBef.TabIndex = 3;
            this.btnBef.Text = "前";
            this.btnBef.UseVisualStyleBackColor = true;
            this.btnBef.Click += new System.EventHandler(this.btnBef_Click);
            // 
            // btnGetAllStock
            // 
            this.btnGetAllStock.Location = new System.Drawing.Point(11, 6);
            this.btnGetAllStock.Name = "btnGetAllStock";
            this.btnGetAllStock.Size = new System.Drawing.Size(74, 28);
            this.btnGetAllStock.TabIndex = 1;
            this.btnGetAllStock.Text = "获取数据";
            this.btnGetAllStock.UseVisualStyleBackColor = true;
            this.btnGetAllStock.Click += new System.EventHandler(this.btnGetAllStock_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(100, 6);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(74, 28);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "画趋势图";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // pnlBody
            // 
            this.pnlBody.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBody.Controls.Add(this.imgBody);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(602, 322);
            this.pnlBody.TabIndex = 1;
            // 
            // imgBody
            // 
            this.imgBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgBody.Location = new System.Drawing.Point(0, 0);
            this.imgBody.Name = "imgBody";
            this.imgBody.Size = new System.Drawing.Size(600, 320);
            this.imgBody.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgBody.TabIndex = 0;
            this.imgBody.TabStop = false;
            this.imgBody.MouseLeave += new System.EventHandler(this.imgBody_MouseLeave);
            this.imgBody.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseMove);
            // 
            // CreateQushi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 387);
            this.Name = "CreateQushi";
            this.Text = "CreateQushi";
            this.pnlTopBody.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.pnlButton.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgBody)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnGetAllStock;
        private System.Windows.Forms.PictureBox imgBody;
        private System.Windows.Forms.Button btnBef;
        private System.Windows.Forms.Button btnAft;
        private System.Windows.Forms.Label lblCntInfo;
        private System.Windows.Forms.ComboBox cmbCon;
        private System.Windows.Forms.Label lblConSel;
        private System.Windows.Forms.CheckBox chkNoChuangye;
    }
}