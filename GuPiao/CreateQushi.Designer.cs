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
            this.btnChgTime = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChgDisp = new System.Windows.Forms.Button();
            this.txtCdSearch = new System.Windows.Forms.TextBox();
            this.lblConSel = new System.Windows.Forms.Label();
            this.cmbCon = new System.Windows.Forms.ComboBox();
            this.lblCntInfo = new System.Windows.Forms.Label();
            this.btnAft = new System.Windows.Forms.Button();
            this.btnBef = new System.Windows.Forms.Button();
            this.btnGetAllStock = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.imgBody = new System.Windows.Forms.PictureBox();
            this.btnTestRun = new System.Windows.Forms.Button();
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
            this.pnlTopBody.Margin = new System.Windows.Forms.Padding(4);
            this.pnlTopBody.Size = new System.Drawing.Size(602, 477);
            // 
            // pnlButton
            // 
            this.pnlButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlButton.Controls.Add(this.btnTestRun);
            this.pnlButton.Controls.Add(this.btnChgTime);
            this.pnlButton.Controls.Add(this.label1);
            this.pnlButton.Controls.Add(this.btnChgDisp);
            this.pnlButton.Controls.Add(this.txtCdSearch);
            this.pnlButton.Controls.Add(this.lblConSel);
            this.pnlButton.Controls.Add(this.cmbCon);
            this.pnlButton.Controls.Add(this.lblCntInfo);
            this.pnlButton.Controls.Add(this.btnAft);
            this.pnlButton.Controls.Add(this.btnBef);
            this.pnlButton.Controls.Add(this.btnGetAllStock);
            this.pnlButton.Controls.Add(this.btnCreate);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 402);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(602, 75);
            this.pnlButton.TabIndex = 0;
            // 
            // btnChgTime
            // 
            this.btnChgTime.Location = new System.Drawing.Point(300, 6);
            this.btnChgTime.Name = "btnChgTime";
            this.btnChgTime.Size = new System.Drawing.Size(37, 62);
            this.btnChgTime.TabIndex = 13;
            this.btnChgTime.Text = "Day";
            this.btnChgTime.UseVisualStyleBackColor = true;
            this.btnChgTime.Click += new System.EventHandler(this.btnChgTime_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(99, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "单个过滤";
            // 
            // btnChgDisp
            // 
            this.btnChgDisp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChgDisp.Location = new System.Drawing.Point(533, 41);
            this.btnChgDisp.Name = "btnChgDisp";
            this.btnChgDisp.Size = new System.Drawing.Size(56, 28);
            this.btnChgDisp.TabIndex = 11;
            this.btnChgDisp.Text = "展 开";
            this.btnChgDisp.UseVisualStyleBackColor = true;
            this.btnChgDisp.Click += new System.EventHandler(this.btnChgDisp_Click);
            // 
            // txtCdSearch
            // 
            this.txtCdSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCdSearch.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCdSearch.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtCdSearch.Location = new System.Drawing.Point(154, 42);
            this.txtCdSearch.Margin = new System.Windows.Forms.Padding(2);
            this.txtCdSearch.MaxLength = 6;
            this.txtCdSearch.Name = "txtCdSearch";
            this.txtCdSearch.Size = new System.Drawing.Size(80, 21);
            this.txtCdSearch.TabIndex = 10;
            this.txtCdSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCdSearch_KeyPress);
            // 
            // lblConSel
            // 
            this.lblConSel.AutoSize = true;
            this.lblConSel.Location = new System.Drawing.Point(99, 15);
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
            "显示日线",
            "显示5分钟线",
            "显示15分钟线",
            "显示30分钟线",
            "查看买点",
            "查看次新",
            "下跌递减",
            "下跌转折",
            "下跌转折两天",
            "下跌转折三天",
            "上涨转折",
            "一路下跌",
            "一路上涨"});
            this.cmbCon.Location = new System.Drawing.Point(154, 11);
            this.cmbCon.Name = "cmbCon";
            this.cmbCon.Size = new System.Drawing.Size(95, 20);
            this.cmbCon.TabIndex = 7;
            // 
            // lblCntInfo
            // 
            this.lblCntInfo.AutoSize = true;
            this.lblCntInfo.Location = new System.Drawing.Point(375, 32);
            this.lblCntInfo.Name = "lblCntInfo";
            this.lblCntInfo.Size = new System.Drawing.Size(59, 12);
            this.lblCntInfo.TabIndex = 6;
            this.lblCntInfo.Text = "9999/9999";
            // 
            // btnAft
            // 
            this.btnAft.Location = new System.Drawing.Point(343, 24);
            this.btnAft.Name = "btnAft";
            this.btnAft.Size = new System.Drawing.Size(26, 28);
            this.btnAft.TabIndex = 4;
            this.btnAft.Text = "后";
            this.btnAft.UseVisualStyleBackColor = true;
            this.btnAft.Click += new System.EventHandler(this.btnAft_Click);
            // 
            // btnBef
            // 
            this.btnBef.Location = new System.Drawing.Point(268, 24);
            this.btnBef.Name = "btnBef";
            this.btnBef.Size = new System.Drawing.Size(26, 28);
            this.btnBef.TabIndex = 3;
            this.btnBef.Text = "前";
            this.btnBef.UseVisualStyleBackColor = true;
            this.btnBef.Click += new System.EventHandler(this.btnBef_Click);
            // 
            // btnGetAllStock
            // 
            this.btnGetAllStock.Location = new System.Drawing.Point(10, 6);
            this.btnGetAllStock.Name = "btnGetAllStock";
            this.btnGetAllStock.Size = new System.Drawing.Size(64, 28);
            this.btnGetAllStock.TabIndex = 1;
            this.btnGetAllStock.Text = "获取数据";
            this.btnGetAllStock.UseVisualStyleBackColor = true;
            this.btnGetAllStock.Click += new System.EventHandler(this.btnGetAllStock_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(10, 40);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(64, 28);
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
            this.pnlBody.Size = new System.Drawing.Size(602, 402);
            this.pnlBody.TabIndex = 1;
            // 
            // imgBody
            // 
            this.imgBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgBody.Location = new System.Drawing.Point(0, 0);
            this.imgBody.Name = "imgBody";
            this.imgBody.Size = new System.Drawing.Size(600, 400);
            this.imgBody.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgBody.TabIndex = 0;
            this.imgBody.TabStop = false;
            this.imgBody.MouseLeave += new System.EventHandler(this.imgBody_MouseLeave);
            this.imgBody.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseMove);
            this.imgBody.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseDown);
            this.imgBody.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseUp);
            // 
            // btnTestRun
            // 
            this.btnTestRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestRun.Location = new System.Drawing.Point(461, 41);
            this.btnTestRun.Name = "btnTestRun";
            this.btnTestRun.Size = new System.Drawing.Size(56, 28);
            this.btnTestRun.TabIndex = 14;
            this.btnTestRun.Text = "测 试";
            this.btnTestRun.UseVisualStyleBackColor = true;
            this.btnTestRun.Click += new System.EventHandler(this.btnTestRun_Click);
            // 
            // CreateQushi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 502);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(608, 530);
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
        private System.Windows.Forms.TextBox txtCdSearch;
        private System.Windows.Forms.Button btnChgDisp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnChgTime;
        private System.Windows.Forms.Button btnTestRun;
    }
}