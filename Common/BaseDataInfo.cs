using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 基础数据情报
    /// </summary>
    public class BaseDataInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

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
        /// 当前点的分型
        /// </summary>
        public PointType CurPointType { get; set; }

        /// <summary>
        /// 下一个点的分型
        /// </summary>
        public PointType NextPointType { get; set; }
    }
}
