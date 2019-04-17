namespace GuPiao
{
    partial class AutoTrade
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
            this.btnRun = new System.Windows.Forms.Button();
            this.chkChuangYe = new System.Windows.Forms.CheckBox();
            this.chkLiangRong = new System.Windows.Forms.CheckBox();
            this.btnViewToday = new System.Windows.Forms.Button();
            this.btnQushi = new System.Windows.Forms.Button();
            this.rdoReal = new System.Windows.Forms.RadioButton();
            this.rdoEmu = new System.Windows.Forms.RadioButton();
            this.dtEmu = new System.Windows.Forms.DateTimePicker();
            this.rdoRealEmu = new System.Windows.Forms.RadioButton();
            this.pnlTopBody.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTopBody
            // 
            this.pnlTopBody.Controls.Add(this.rdoRealEmu);
            this.pnlTopBody.Controls.Add(this.dtEmu);
            this.pnlTopBody.Controls.Add(this.rdoEmu);
            this.pnlTopBody.Controls.Add(this.rdoReal);
            this.pnlTopBody.Controls.Add(this.btnQushi);
            this.pnlTopBody.Controls.Add(this.btnViewToday);
            this.pnlTopBody.Controls.Add(this.chkLiangRong);
            this.pnlTopBody.Controls.Add(this.chkChuangYe);
            this.pnlTopBody.Controls.Add(this.btnRun);
            this.pnlTopBody.Size = new System.Drawing.Size(347, 127);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(12, 12);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(83, 33);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "开始运行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // chkChuangYe
            // 
            this.chkChuangYe.AutoSize = true;
            this.chkChuangYe.Location = new System.Drawing.Point(22, 51);
            this.chkChuangYe.Name = "chkChuangYe";
            this.chkChuangYe.Size = new System.Drawing.Size(96, 16);
            this.chkChuangYe.TabIndex = 1;
            this.chkChuangYe.Text = "包括创业数据";
            this.chkChuangYe.UseVisualStyleBackColor = true;
            // 
            // chkLiangRong
            // 
            this.chkLiangRong.AutoSize = true;
            this.chkLiangRong.Location = new System.Drawing.Point(22, 73);
            this.chkLiangRong.Name = "chkLiangRong";
            this.chkLiangRong.Size = new System.Drawing.Size(96, 16);
            this.chkLiangRong.TabIndex = 2;
            this.chkLiangRong.Text = "包括两融数据";
            this.chkLiangRong.UseVisualStyleBackColor = true;
            // 
            // btnViewToday
            // 
            this.btnViewToday.Enabled = false;
            this.btnViewToday.Location = new System.Drawing.Point(130, 12);
            this.btnViewToday.Name = "btnViewToday";
            this.btnViewToday.Size = new System.Drawing.Size(83, 33);
            this.btnViewToday.TabIndex = 3;
            this.btnViewToday.Text = "当日情况";
            this.btnViewToday.UseVisualStyleBackColor = true;
            // 
            // btnQushi
            // 
            this.btnQushi.Location = new System.Drawing.Point(250, 12);
            this.btnQushi.Name = "btnQushi";
            this.btnQushi.Size = new System.Drawing.Size(83, 33);
            this.btnQushi.TabIndex = 4;
            this.btnQushi.Text = "趋势辅助";
            this.btnQushi.UseVisualStyleBackColor = true;
            // 
            // rdoReal
            // 
            this.rdoReal.AutoSize = true;
            this.rdoReal.Location = new System.Drawing.Point(148, 50);
            this.rdoReal.Name = "rdoReal";
            this.rdoReal.Size = new System.Drawing.Size(95, 16);
            this.rdoReal.TabIndex = 5;
            this.rdoReal.Text = "实时真实处理";
            this.rdoReal.UseVisualStyleBackColor = true;
            // 
            // rdoEmu
            // 
            this.rdoEmu.AutoSize = true;
            this.rdoEmu.Checked = true;
            this.rdoEmu.Location = new System.Drawing.Point(148, 95);
            this.rdoEmu.Name = "rdoEmu";
            this.rdoEmu.Size = new System.Drawing.Size(95, 16);
            this.rdoEmu.TabIndex = 6;
            this.rdoEmu.TabStop = true;
            this.rdoEmu.Text = "历史模拟处理";
            this.rdoEmu.UseVisualStyleBackColor = true;
            // 
            // dtEmu
            // 
            this.dtEmu.Location = new System.Drawing.Point(22, 95);
            this.dtEmu.Name = "dtEmu";
            this.dtEmu.Size = new System.Drawing.Size(104, 19);
            this.dtEmu.TabIndex = 7;
            this.dtEmu.Value = new System.DateTime(2019, 3, 18, 17, 11, 0, 0);
            // 
            // rdoRealEmu
            // 
            this.rdoRealEmu.AutoSize = true;
            this.rdoRealEmu.Location = new System.Drawing.Point(148, 72);
            this.rdoRealEmu.Name = "rdoRealEmu";
            this.rdoRealEmu.Size = new System.Drawing.Size(95, 16);
            this.rdoRealEmu.TabIndex = 8;
            this.rdoRealEmu.Text = "实时模拟处理";
            this.rdoRealEmu.UseVisualStyleBackColor = true;
            // 
            // AutoTrade
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 152);
            this.MaximizeBox = false;
            this.Name = "AutoTrade";
            this.Text = "AutoTrade";
            this.pnlTopBody.ResumeLayout(false);
            this.pnlTopBody.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.CheckBox chkChuangYe;
        private System.Windows.Forms.CheckBox chkLiangRong;
        private System.Windows.Forms.Button btnViewToday;
        private System.Windows.Forms.Button btnQushi;
        private System.Windows.Forms.RadioButton rdoEmu;
        private System.Windows.Forms.RadioButton rdoReal;
        private System.Windows.Forms.DateTimePicker dtEmu;
        private System.Windows.Forms.RadioButton rdoRealEmu;
    }
}