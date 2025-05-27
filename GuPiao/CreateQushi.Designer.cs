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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnViewDiff = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtEnd = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtStart = new System.Windows.Forms.DateTimePicker();
            this.btnSaveData = new System.Windows.Forms.Button();
            this.cmbData = new System.Windows.Forms.ComboBox();
            this.btnDateAft = new System.Windows.Forms.Button();
            this.btnDateBef = new System.Windows.Forms.Button();
            this.btnTestRun = new System.Windows.Forms.Button();
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
            this.pnlTopBody.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBody)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTopBody
            // 
            this.pnlTopBody.Controls.Add(this.pnlBody);
            this.pnlTopBody.Controls.Add(this.pnlButton);
            this.pnlTopBody.Margin = new System.Windows.Forms.Padding(4);
            this.pnlTopBody.Size = new System.Drawing.Size(1274, 576);
            // 
            // pnlButton
            // 
            this.pnlButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlButton.Controls.Add(this.panel1);
            this.pnlButton.Controls.Add(this.cmbData);
            this.pnlButton.Controls.Add(this.btnDateAft);
            this.pnlButton.Controls.Add(this.btnDateBef);
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
            this.pnlButton.Location = new System.Drawing.Point(0, 501);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(1274, 75);
            this.pnlButton.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnViewDiff);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.dtEnd);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.dtStart);
            this.panel1.Controls.Add(this.btnSaveData);
            this.panel1.Location = new System.Drawing.Point(519, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(299, 75);
            this.panel1.TabIndex = 20;
            // 
            // btnViewDiff
            // 
            this.btnViewDiff.Location = new System.Drawing.Point(190, 38);
            this.btnViewDiff.Name = "btnViewDiff";
            this.btnViewDiff.Size = new System.Drawing.Size(90, 24);
            this.btnViewDiff.TabIndex = 24;
            this.btnViewDiff.Text = "对比两点趋势";
            this.btnViewDiff.UseVisualStyleBackColor = true;
            this.btnViewDiff.Click += new System.EventHandler(this.btnViewDiff_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "结束日期";
            // 
            // dtEnd
            // 
            this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtEnd.Location = new System.Drawing.Point(73, 39);
            this.dtEnd.Name = "dtEnd";
            this.dtEnd.Size = new System.Drawing.Size(99, 19);
            this.dtEnd.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "开始日期";
            // 
            // dtStart
            // 
            this.dtStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtStart.Location = new System.Drawing.Point(73, 14);
            this.dtStart.Name = "dtStart";
            this.dtStart.Size = new System.Drawing.Size(99, 19);
            this.dtStart.TabIndex = 20;
            // 
            // btnSaveData
            // 
            this.btnSaveData.Location = new System.Drawing.Point(190, 11);
            this.btnSaveData.Name = "btnSaveData";
            this.btnSaveData.Size = new System.Drawing.Size(90, 24);
            this.btnSaveData.TabIndex = 19;
            this.btnSaveData.Text = "保存历史趋势";
            this.btnSaveData.UseVisualStyleBackColor = true;
            this.btnSaveData.Click += new System.EventHandler(this.btnSaveData_Click);
            // 
            // cmbData
            // 
            this.cmbData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbData.FormattingEnabled = true;
            this.cmbData.Items.AddRange(new object[] {
            "2020-04-18"});
            this.cmbData.Location = new System.Drawing.Point(300, 47);
            this.cmbData.Name = "cmbData";
            this.cmbData.Size = new System.Drawing.Size(105, 20);
            this.cmbData.TabIndex = 18;
            this.cmbData.SelectedIndexChanged += new System.EventHandler(this.cmbData_SelectedIndexChanged);
            // 
            // btnDateAft
            // 
            this.btnDateAft.Font = new System.Drawing.Font("MS UI Gothic", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnDateAft.Location = new System.Drawing.Point(355, 27);
            this.btnDateAft.Name = "btnDateAft";
            this.btnDateAft.Size = new System.Drawing.Size(47, 19);
            this.btnDateAft.TabIndex = 17;
            this.btnDateAft.Text = "右移动";
            this.btnDateAft.UseVisualStyleBackColor = true;
            this.btnDateAft.Click += new System.EventHandler(this.btnDateAft_Click);
            // 
            // btnDateBef
            // 
            this.btnDateBef.Font = new System.Drawing.Font("MS UI Gothic", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnDateBef.Location = new System.Drawing.Point(303, 27);
            this.btnDateBef.Name = "btnDateBef";
            this.btnDateBef.Size = new System.Drawing.Size(47, 19);
            this.btnDateBef.TabIndex = 16;
            this.btnDateBef.Text = "左移动";
            this.btnDateBef.UseVisualStyleBackColor = true;
            this.btnDateBef.Click += new System.EventHandler(this.btnDateBef_Click);
            // 
            // btnTestRun
            // 
            this.btnTestRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestRun.Location = new System.Drawing.Point(1143, 41);
            this.btnTestRun.Name = "btnTestRun";
            this.btnTestRun.Size = new System.Drawing.Size(56, 28);
            this.btnTestRun.TabIndex = 14;
            this.btnTestRun.Text = "测 试";
            this.btnTestRun.UseVisualStyleBackColor = true;
            this.btnTestRun.Click += new System.EventHandler(this.btnTestRun_Click);
            // 
            // btnChgTime
            // 
            this.btnChgTime.Location = new System.Drawing.Point(300, 6);
            this.btnChgTime.Name = "btnChgTime";
            this.btnChgTime.Size = new System.Drawing.Size(105, 21);
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
            this.btnChgDisp.Location = new System.Drawing.Point(1205, 41);
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
            this.txtCdSearch.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCdSearch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
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
            "第一类买点",
            "一路上涨",
            "最后是顶分型",
            "连续涨停",
            "一路下跌",
            "最后是底分型",
            "连续跌停",
            "查看龙头股",
            "所有天数据",
            "所有5分钟数据",
            "所有30分钟数据",
            "查看成功买点",
            "查看失败买点",
            "查看买点",
            "查看次新",
            "下跌递减",
            "下跌转折两天"});
            this.cmbCon.Location = new System.Drawing.Point(154, 11);
            this.cmbCon.Name = "cmbCon";
            this.cmbCon.Size = new System.Drawing.Size(95, 20);
            this.cmbCon.TabIndex = 7;
            // 
            // lblCntInfo
            // 
            this.lblCntInfo.AutoSize = true;
            this.lblCntInfo.Location = new System.Drawing.Point(442, 32);
            this.lblCntInfo.Name = "lblCntInfo";
            this.lblCntInfo.Size = new System.Drawing.Size(59, 12);
            this.lblCntInfo.TabIndex = 6;
            this.lblCntInfo.Text = "9999/9999";
            // 
            // btnAft
            // 
            this.btnAft.Location = new System.Drawing.Point(410, 24);
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
            this.btnGetAllStock.Visible = false;
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
            this.btnCreate.Visible = false;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // pnlBody
            // 
            this.pnlBody.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBody.Controls.Add(this.imgBody);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(1274, 501);
            this.pnlBody.TabIndex = 1;
            // 
            // imgBody
            // 
            this.imgBody.Location = new System.Drawing.Point(0, 0);
            this.imgBody.Name = "imgBody";
            this.imgBody.Size = new System.Drawing.Size(1272, 500);
            this.imgBody.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgBody.TabIndex = 0;
            this.imgBody.TabStop = false;
            this.imgBody.MouseLeave += new System.EventHandler(this.imgBody_MouseLeave);
            this.imgBody.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseMove);
            this.imgBody.MouseClick += new System.Windows.Forms.MouseEventHandler(this.imgBody_MouseClick);
            // 
            // CreateQushi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1274, 601);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1280, 630);
            this.Name = "CreateQushi";
            this.Text = "CreateQushi";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreateQushi_FormClosing);
            this.pnlTopBody.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.pnlButton.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Button btnDateBef;
        private System.Windows.Forms.Button btnDateAft;
        private System.Windows.Forms.ComboBox cmbData;
        private System.Windows.Forms.Button btnSaveData;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtStart;
        private System.Windows.Forms.Button btnViewDiff;
    }
}