namespace GuPiao
{
    partial class ReadTimeInfo
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grdGuPiao = new System.Windows.Forms.DataGridView();
            this.bianHao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zuoriVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yinkuiPer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CanUseCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buyFlg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.autoBuy = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.autoSell = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).BeginInit();
            this.SuspendLayout();
            // 
            // grdGuPiao
            // 
            this.grdGuPiao.AllowUserToAddRows = false;
            this.grdGuPiao.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grdGuPiao.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdGuPiao.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bianHao,
            this.zuoriVal,
            this.currentVal,
            this.yinkuiPer,
            this.totalCount,
            this.CanUseCount,
            this.buyFlg,
            this.autoBuy,
            this.autoSell});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grdGuPiao.DefaultCellStyle = dataGridViewCellStyle8;
            this.grdGuPiao.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdGuPiao.Location = new System.Drawing.Point(0, 0);
            this.grdGuPiao.Name = "grdGuPiao";
            this.grdGuPiao.ReadOnly = true;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.grdGuPiao.RowHeadersVisible = false;
            this.grdGuPiao.RowTemplate.Height = 21;
            this.grdGuPiao.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdGuPiao.Size = new System.Drawing.Size(349, 191);
            this.grdGuPiao.TabIndex = 1;
            // 
            // bianHao
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.bianHao.DefaultCellStyle = dataGridViewCellStyle2;
            this.bianHao.HeaderText = "bianHao";
            this.bianHao.Name = "bianHao";
            this.bianHao.ReadOnly = true;
            // 
            // zuoriVal
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.zuoriVal.DefaultCellStyle = dataGridViewCellStyle3;
            this.zuoriVal.HeaderText = "zuoriVal";
            this.zuoriVal.Name = "zuoriVal";
            this.zuoriVal.ReadOnly = true;
            this.zuoriVal.Width = 80;
            // 
            // currentVal
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.currentVal.DefaultCellStyle = dataGridViewCellStyle4;
            this.currentVal.HeaderText = "currentVal";
            this.currentVal.Name = "currentVal";
            this.currentVal.ReadOnly = true;
            this.currentVal.Width = 80;
            // 
            // yinkuiPer
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.yinkuiPer.DefaultCellStyle = dataGridViewCellStyle5;
            this.yinkuiPer.HeaderText = "yinkuiPer";
            this.yinkuiPer.Name = "yinkuiPer";
            this.yinkuiPer.ReadOnly = true;
            this.yinkuiPer.Width = 80;
            // 
            // totalCount
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.totalCount.DefaultCellStyle = dataGridViewCellStyle6;
            this.totalCount.HeaderText = "totalCount";
            this.totalCount.Name = "totalCount";
            this.totalCount.ReadOnly = true;
            this.totalCount.Visible = false;
            this.totalCount.Width = 80;
            // 
            // CanUseCount
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.CanUseCount.DefaultCellStyle = dataGridViewCellStyle7;
            this.CanUseCount.HeaderText = "CanUseCount";
            this.CanUseCount.Name = "CanUseCount";
            this.CanUseCount.ReadOnly = true;
            this.CanUseCount.Visible = false;
            this.CanUseCount.Width = 80;
            // 
            // buyFlg
            // 
            this.buyFlg.HeaderText = "buyFlg";
            this.buyFlg.Name = "buyFlg";
            this.buyFlg.ReadOnly = true;
            this.buyFlg.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.buyFlg.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.buyFlg.Visible = false;
            this.buyFlg.Width = 60;
            // 
            // autoBuy
            // 
            this.autoBuy.HeaderText = "autoBuy";
            this.autoBuy.Name = "autoBuy";
            this.autoBuy.ReadOnly = true;
            this.autoBuy.Visible = false;
            this.autoBuy.Width = 60;
            // 
            // autoSell
            // 
            this.autoSell.HeaderText = "autoSell";
            this.autoSell.Name = "autoSell";
            this.autoSell.ReadOnly = true;
            this.autoSell.Visible = false;
            this.autoSell.Width = 60;
            // 
            // ReadTimeInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 191);
            this.Controls.Add(this.grdGuPiao);
            this.MaximizeBox = false;
            this.Name = "ReadTimeInfo";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "实时信息";
            this.Shown += new System.EventHandler(this.GuPiaoTool_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grdGuPiao;
        private System.Windows.Forms.DataGridViewTextBoxColumn bianHao;
        private System.Windows.Forms.DataGridViewTextBoxColumn zuoriVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn yinkuiPer;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn CanUseCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn buyFlg;
        private System.Windows.Forms.DataGridViewCheckBoxColumn autoBuy;
        private System.Windows.Forms.DataGridViewCheckBoxColumn autoSell;
    }
}

