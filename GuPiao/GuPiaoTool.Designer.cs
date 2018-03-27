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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnRun = new System.Windows.Forms.Button();
            this.grdGuPiao = new System.Windows.Forms.DataGridView();
            this.bianHao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zuoriVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yinkuiPer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(133, 178);
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
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle15;
            this.grdGuPiao.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdGuPiao.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bianHao,
            this.zuoriVal,
            this.currentVal,
            this.yinkuiPer});
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle20.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grdGuPiao.DefaultCellStyle = dataGridViewCellStyle20;
            this.grdGuPiao.Location = new System.Drawing.Point(12, 12);
            this.grdGuPiao.Name = "grdGuPiao";
            this.grdGuPiao.ReadOnly = true;
            dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle21.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle21.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle21.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle21.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle21.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle21.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdGuPiao.RowHeadersDefaultCellStyle = dataGridViewCellStyle21;
            this.grdGuPiao.RowHeadersVisible = false;
            this.grdGuPiao.RowTemplate.Height = 21;
            this.grdGuPiao.Size = new System.Drawing.Size(411, 147);
            this.grdGuPiao.TabIndex = 1;
            // 
            // bianHao
            // 
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.bianHao.DefaultCellStyle = dataGridViewCellStyle16;
            this.bianHao.HeaderText = "bianHao";
            this.bianHao.Name = "bianHao";
            this.bianHao.ReadOnly = true;
            // 
            // zuoriVal
            // 
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle17.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.zuoriVal.DefaultCellStyle = dataGridViewCellStyle17;
            this.zuoriVal.HeaderText = "zuoriVal";
            this.zuoriVal.Name = "zuoriVal";
            this.zuoriVal.ReadOnly = true;
            // 
            // currentVal
            // 
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle18.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.currentVal.DefaultCellStyle = dataGridViewCellStyle18;
            this.currentVal.HeaderText = "currentVal";
            this.currentVal.Name = "currentVal";
            this.currentVal.ReadOnly = true;
            // 
            // yinkuiPer
            // 
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle19.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.yinkuiPer.DefaultCellStyle = dataGridViewCellStyle19;
            this.yinkuiPer.HeaderText = "yinkuiPer";
            this.yinkuiPer.Name = "yinkuiPer";
            this.yinkuiPer.ReadOnly = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Enabled = false;
            this.btnRefresh.Location = new System.Drawing.Point(227, 178);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 30);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷  新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // GuPiaoTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 220);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.grdGuPiao);
            this.Controls.Add(this.btnRun);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GuPiaoTool";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Get Money";
            ((System.ComponentModel.ISupportInitialize)(this.grdGuPiao)).EndInit();
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
    }
}

