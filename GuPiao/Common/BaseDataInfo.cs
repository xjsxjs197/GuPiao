using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuPiao.Common
{
    /// <summary>
    /// 基础数据情报
    /// </summary>
    public class BaseDataInfo
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 当天价位
        /// </summary>
        public decimal DayVal { get; set; }

        /// <summary>
        /// 当天最低价位
        /// </summary>
        public decimal DayMinVal { get; set; }

        /// <summary>
        /// 当天最高价位
        /// </summary>
        public decimal DayMaxVal { get; set; }

        /// <summary>
        /// 当前的笔的状态
        /// </summary>
        public PenStatus CurPen { get; set; }

        /// <summary>
        /// 下一个笔的状态
        /// </summary>
        public PenStatus NextPen { get; set; }
    }
}
