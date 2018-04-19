namespace GuPiaoTool
{
    partial class GuPiaoTool
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnRun = new System.Windows.Forms.Button();
            this.grdGuPiao = new System.Windows.Forms.DataGridView();
            this.pnlBtn = new System.Windows.Forms.Panel();
            this.pnlBaseInfo = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbAccountType = new System.Windows.Forms.ComboBox();
            this.cmbBrokerType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblCanUse = new System.Windows.Forms.Label();
            this.rdoSync = new System.Windows.Forms.RadioButton();
            this.rdoNotSync = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbCountBuy = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPriceBuy = new System.Windows.Forms.TextBox();
            this.btnBuy = new System.Windows.Forms.Button();
            this.btnQuictBuy = new System.Windows.Forms.Button();
            this.btnQuickSell = new System.Windows.Forms.Button();
            this.btnSell = new System.Windows.Forms.Button();
            this.txtPriceSell = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbCountSell = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.bianHao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zuoriVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yinkuiPer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CanUseCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).BeginInit();
            this.pnlBtn.SuspendLayout();
            this.pnlBaseInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(8, 14);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(70, 30);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "运  行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // grdGuPiao
            // 
            this.grdGuPiao.AllowUserToAddRows = false;
            this.grdGuPiao.AllowUserToDeleteRows = false;
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle19;
            this.grdGuPiao.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdGuPiao.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bianHao,
            this.zuoriVal,
            this.currentVal,
            this.yinkuiPer,
            this.totalCount,
            this.CanUseCount});
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle26.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grdGuPiao.DefaultCellStyle = dataGridViewCellStyle26;
            this.grdGuPiao.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdGuPiao.Location = new System.Drawing.Point(0, 0);
            this.grdGuPiao.Name = "grdGuPiao";
            this.grdGuPiao.ReadOnly = true;
            dataGridViewCellStyle27.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle27.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle27.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle27.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle27.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle27.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle27.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.RowHeadersDefaultCellStyle = dataGridViewCellStyle27;
            this.grdGuPiao.RowHeadersVisible = false;
            this.grdGuPiao.RowTemplate.Height = 21;
            this.grdGuPiao.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdGuPiao.Size = new System.Drawing.Size(572, 248);
            this.grdGuPiao.TabIndex = 1;
            // 
            // pnlBtn
            // 
            this.pnlBtn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBtn.Controls.Add(this.txtPriceSell);
            this.pnlBtn.Controls.Add(this.label7);
            this.pnlBtn.Controls.Add(this.cmbCountSell);
            this.pnlBtn.Controls.Add(this.label8);
            this.pnlBtn.Controls.Add(this.btnQuickSell);
            this.pnlBtn.Controls.Add(this.btnSell);
            this.pnlBtn.Controls.Add(this.btnQuictBuy);
            this.pnlBtn.Controls.Add(this.btnBuy);
            this.pnlBtn.Controls.Add(this.txtPriceBuy);
            this.pnlBtn.Controls.Add(this.label6);
            this.pnlBtn.Controls.Add(this.cmbCountBuy);
            this.pnlBtn.Controls.Add(this.label5);
            this.pnlBtn.Controls.Add(this.btnRun);
            this.pnlBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBtn.Location = new System.Drawing.Point(0, 248);
            this.pnlBtn.Name = "pnlBtn";
            this.pnlBtn.Size = new System.Drawing.Size(572, 59);
            this.pnlBtn.TabIndex = 3;
            // 
            // pnlBaseInfo
            // 
            this.pnlBaseInfo.Controls.Add(this.rdoNotSync);
            this.pnlBaseInfo.Controls.Add(this.rdoSync);
            this.pnlBaseInfo.Controls.Add(this.lblCanUse);
            this.pnlBaseInfo.Controls.Add(this.cmbBrokerType);
            this.pnlBaseInfo.Controls.Add(this.label4);
            this.pnlBaseInfo.Controls.Add(this.lblTotal);
            this.pnlBaseInfo.Controls.Add(this.cmbAccountType);
            this.pnlBaseInfo.Controls.Add(this.label2);
            this.pnlBaseInfo.Controls.Add(this.label3);
            this.pnlBaseInfo.Controls.Add(this.label1);
            this.pnlBaseInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBaseInfo.Location = new System.Drawing.Point(0, 189);
            this.pnlBaseInfo.Name = "pnlBaseInfo";
            this.pnlBaseInfo.Size = new System.Drawing.Size(572, 59);
            this.pnlBaseInfo.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "AccountType";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "BrokerType";
            // 
            // cmbAccountType
            // 
            this.cmbAccountType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccountType.FormattingEnabled = true;
            this.cmbAccountType.Location = new System.Drawing.Point(84, 7);
            this.cmbAccountType.Name = "cmbAccountType";
            this.cmbAccountType.Size = new System.Drawing.Size(81, 20);
            this.cmbAccountType.TabIndex = 2;
            // 
            // cmbBrokerType
            // 
            this.cmbBrokerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBrokerType.FormattingEnabled = true;
            this.cmbBrokerType.Location = new System.Drawing.Point(84, 31);
            this.cmbBrokerType.Name = "cmbBrokerType";
            this.cmbBrokerType.Size = new System.Drawing.Size(81, 20);
            this.cmbBrokerType.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(289, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "totalMoney";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(289, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "canUseMoney";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(369, 10);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(49, 12);
            this.lblTotal.TabIndex = 3;
            this.lblTotal.Text = "21000.65";
            // 
            // lblCanUse
            // 
            this.lblCanUse.AutoSize = true;
            this.lblCanUse.Location = new System.Drawing.Point(369, 34);
            this.lblCanUse.Name = "lblCanUse";
            this.lblCanUse.Size = new System.Drawing.Size(29, 12);
            this.lblCanUse.TabIndex = 4;
            this.lblCanUse.Text = "2500";
            // 
            // rdoSync
            // 
            this.rdoSync.AutoSize = true;
            this.rdoSync.Enabled = false;
            this.rdoSync.Location = new System.Drawing.Point(201, 8);
            this.rdoSync.Name = "rdoSync";
            this.rdoSync.Size = new System.Drawing.Size(47, 16);
            this.rdoSync.TabIndex = 5;
            this.rdoSync.Text = "同步";
            this.rdoSync.UseVisualStyleBackColor = true;
            // 
            // rdoNotSync
            // 
            this.rdoNotSync.AutoSize = true;
            this.rdoNotSync.Checked = true;
            this.rdoNotSync.Location = new System.Drawing.Point(201, 32);
            this.rdoNotSync.Name = "rdoNotSync";
            this.rdoNotSync.Size = new System.Drawing.Size(47, 16);
            this.rdoNotSync.TabIndex = 6;
            this.rdoNotSync.TabStop = true;
            this.rdoNotSync.Text = "异步";
            this.rdoNotSync.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(105, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "Count";
            // 
            // cmbCountBuy
            // 
            this.cmbCountBuy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCountBuy.Enabled = false;
            this.cmbCountBuy.FormattingEnabled = true;
            this.cmbCountBuy.Items.AddRange(new object[] {
            "100",
            "200",
            "300",
            "400",
            "500",
            "600",
            "700",
            "800",
            "900",
            "1000"});
            this.cmbCountBuy.Location = new System.Drawing.Point(142, 9);
            this.cmbCountBuy.Name = "cmbCountBuy";
            this.cmbCountBuy.Size = new System.Drawing.Size(50, 20);
            this.cmbCountBuy.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(202, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 12);
            this.label6.TabIndex = 4;
            this.label6.Text = "Price";
            // 
            // txtPriceBuy
            // 
            this.txtPriceBuy.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPriceBuy.Location = new System.Drawing.Point(236, 9);
            this.txtPriceBuy.Name = "txtPriceBuy";
            this.txtPriceBuy.ReadOnly = true;
            this.txtPriceBuy.Size = new System.Drawing.Size(48, 19);
            this.txtPriceBuy.TabIndex = 5;
            // 
            // btnBuy
            // 
            this.btnBuy.Enabled = false;
            this.btnBuy.Location = new System.Drawing.Point(290, 8);
            this.btnBuy.Name = "btnBuy";
            this.btnBuy.Size = new System.Drawing.Size(67, 21);
            this.btnBuy.TabIndex = 6;
            this.btnBuy.Text = "Buy";
            this.btnBuy.UseVisualStyleBackColor = true;
            this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
            // 
            // btnQuictBuy
            // 
            this.btnQuictBuy.Enabled = false;
            this.btnQuictBuy.Location = new System.Drawing.Point(360, 8);
            this.btnQuictBuy.Name = "btnQuictBuy";
            this.btnQuictBuy.Size = new System.Drawing.Size(67, 21);
            this.btnQuictBuy.TabIndex = 7;
            this.btnQuictBuy.Text = "QuickBuy";
            this.btnQuictBuy.UseVisualStyleBackColor = true;
            this.btnQuictBuy.Click += new System.EventHandler(this.btnQuictBuy_Click);
            // 
            // btnQuickSell
            // 
            this.btnQuickSell.Enabled = false;
            this.btnQuickSell.Location = new System.Drawing.Point(360, 30);
            this.btnQuickSell.Name = "btnQuickSell";
            this.btnQuickSell.Size = new System.Drawing.Size(67, 21);
            this.btnQuickSell.TabIndex = 9;
            this.btnQuickSell.Text = "QuictSell";
            this.btnQuickSell.UseVisualStyleBackColor = true;
            this.btnQuickSell.Click += new System.EventHandler(this.btnQuickSell_Click);
            // 
            // btnSell
            // 
            this.btnSell.Enabled = false;
            this.btnSell.Location = new System.Drawing.Point(290, 30);
            this.btnSell.Name = "btnSell";
            this.btnSell.Size = new System.Drawing.Size(67, 21);
            this.btnSell.TabIndex = 8;
            this.btnSell.Text = "Sell";
            this.btnSell.UseVisualStyleBackColor = true;
            this.btnSell.Click += new System.EventHandler(this.btnSell_Click);
            // 
            // txtPriceSell
            // 
            this.txtPriceSell.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPriceSell.Location = new System.Drawing.Point(236, 31);
            this.txtPriceSell.Name = "txtPriceSell";
            this.txtPriceSell.ReadOnly = true;
            this.txtPriceSell.Size = new System.Drawing.Size(48, 19);
            this.txtPriceSell.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(202, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "Price";
            // 
            // cmbCountSell
            // 
            this.cmbCountSell.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCountSell.Enabled = false;
            this.cmbCountSell.FormattingEnabled = true;
            this.cmbCountSell.Items.AddRange(new object[] {
            "100",
            "200",
            "300",
            "400",
            "500",
            "600",
            "700",
            "800",
            "900",
            "1000"});
            this.cmbCountSell.Location = new System.Drawing.Point(142, 31);
            this.cmbCountSell.Name = "cmbCountSell";
            this.cmbCountSell.Size = new System.Drawing.Size(50, 20);
            this.cmbCountSell.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(105, 34);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "Count";
            // 
            // bianHao
            // 
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.bianHao.DefaultCellStyle = dataGridViewCellStyle20;
            this.bianHao.HeaderText = "bianHao";
            this.bianHao.Name = "bianHao";
            this.bianHao.ReadOnly = true;
            // 
            // zuoriVal
            // 
            dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle21.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.zuoriVal.DefaultCellStyle = dataGridViewCellStyle21;
            this.zuoriVal.HeaderText = "zuoriVal";
            this.zuoriVal.Name = "zuoriVal";
            this.zuoriVal.ReadOnly = true;
            // 
            // currentVal
            // 
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle22.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.currentVal.DefaultCellStyle = dataGridViewCellStyle22;
            this.currentVal.HeaderText = "currentVal";
            this.currentVal.Name = "currentVal";
            this.currentVal.ReadOnly = true;
            // 
            // yinkuiPer
            // 
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle23.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.yinkuiPer.DefaultCellStyle = dataGridViewCellStyle23;
            this.yinkuiPer.HeaderText = "yinkuiPer";
            this.yinkuiPer.Name = "yinkuiPer";
            this.yinkuiPer.ReadOnly = true;
            // 
            // totalCount
            // 
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.totalCount.DefaultCellStyle = dataGridViewCellStyle24;
            this.totalCount.HeaderText = "totalCount";
            this.totalCount.Name = "totalCount";
            this.totalCount.ReadOnly = true;
            this.totalCount.Width = 80;
            // 
            // CanUseCount
            // 
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.CanUseCount.DefaultCellStyle = dataGridViewCellStyle25;
            this.CanUseCount.HeaderText = "CanUseCount";
            this.CanUseCount.Name = "CanUseCount";
            this.CanUseCount.ReadOnly = true;
            this.CanUseCount.Width = 80;
            // 
            // GuPiaoTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 307);
            this.Controls.Add(this.pnlBaseInfo);
            this.Controls.Add(this.grdGuPiao);
            this.Controls.Add(this.pnlBtn);
            this.MaximizeBox = false;
            this.Name = "GuPiaoTool";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Get Money";
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).EndInit();
            this.pnlBtn.ResumeLayout(false);
            this.pnlBtn.PerformLayout();
            this.pnlBaseInfo.ResumeLayout(false);
            this.pnlBaseInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DataGridView grdGuPiao;
        private System.Windows.Forms.Panel pnlBtn;
        private System.Windows.Forms.Panel pnlBaseInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbAccountType;
        private System.Windows.Forms.ComboBox cmbBrokerType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCanUse;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.RadioButton rdoNotSync;
        private System.Windows.Forms.RadioButton rdoSync;
        private System.Windows.Forms.ComboBox cmbCountBuy;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPriceBuy;
        private System.Windows.Forms.Button btnBuy;
        private System.Windows.Forms.Button btnQuictBuy;
        private System.Windows.Forms.Button btnQuickSell;
        private System.Windows.Forms.Button btnSell;
        private System.Windows.Forms.TextBox txtPriceSell;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbCountSell;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridViewTextBoxColumn bianHao;
        private System.Windows.Forms.DataGridViewTextBoxColumn zuoriVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn yinkuiPer;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn CanUseCount;
    }
}

