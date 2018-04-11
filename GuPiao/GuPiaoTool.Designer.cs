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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnRun = new System.Windows.Forms.Button();
            this.grdGuPiao = new System.Windows.Forms.DataGridView();
            this.bianHao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zuoriVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yinkuiPer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(101, 10);
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
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.grdGuPiao.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdGuPiao.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bianHao,
            this.zuoriVal,
            this.currentVal,
            this.yinkuiPer});
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grdGuPiao.DefaultCellStyle = dataGridViewCellStyle13;
            this.grdGuPiao.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdGuPiao.Location = new System.Drawing.Point(0, 0);
            this.grdGuPiao.Name = "grdGuPiao";
            this.grdGuPiao.ReadOnly = true;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.RowHeadersDefaultCellStyle = dataGridViewCellStyle14;
            this.grdGuPiao.RowHeadersVisible = false;
            this.grdGuPiao.RowTemplate.Height = 21;
            this.grdGuPiao.Size = new System.Drawing.Size(435, 171);
            this.grdGuPiao.TabIndex = 1;
            // 
            // bianHao
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.bianHao.DefaultCellStyle = dataGridViewCellStyle9;
            this.bianHao.HeaderText = "bianHao";
            this.bianHao.Name = "bianHao";
            this.bianHao.ReadOnly = true;
            // 
            // zuoriVal
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle10.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.zuoriVal.DefaultCellStyle = dataGridViewCellStyle10;
            this.zuoriVal.HeaderText = "zuoriVal";
            this.zuoriVal.Name = "zuoriVal";
            this.zuoriVal.ReadOnly = true;
            // 
            // currentVal
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle11.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.currentVal.DefaultCellStyle = dataGridViewCellStyle11;
            this.currentVal.HeaderText = "currentVal";
            this.currentVal.Name = "currentVal";
            this.currentVal.ReadOnly = true;
            // 
            // yinkuiPer
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle12.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.yinkuiPer.DefaultCellStyle = dataGridViewCellStyle12;
            this.yinkuiPer.HeaderText = "yinkuiPer";
            this.yinkuiPer.Name = "yinkuiPer";
            this.yinkuiPer.ReadOnly = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Enabled = false;
            this.btnRefresh.Location = new System.Drawing.Point(271, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 30);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷  新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.btnRun);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 171);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(435, 49);
            this.panel1.TabIndex = 3;
            // 
            // GuPiaoTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 220);
            this.Controls.Add(this.grdGuPiao);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "GuPiaoTool";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Get Money";
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DataGridView grdGuPiao;
        private System.Windows.Forms.DataGridViewTextBoxColumn bianHao;
        private System.Windows.Forms.DataGridViewTextBoxColumn zuoriVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentVal;
        private System.Windows.Forms.DataGridViewTextBoxColumn yinkuiPer;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Panel panel1;
    }
}

