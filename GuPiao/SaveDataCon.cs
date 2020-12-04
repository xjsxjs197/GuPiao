using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GuPiao
{
    /// <summary>
    /// 跟踪当前数据
    /// </summary>
    public partial class formHstData : Form
    {
        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public formHstData(string stockCd)
        {
            InitializeComponent();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="stockCd"></param>
        private void InitPage(string stockCd)
        { 
            //DateTime now = DateTime.Now;
            //this.dtEnd.Value = now;
            //this.dtStart.Value = now.AddDays(-7);
        }

        #endregion
    }
}
