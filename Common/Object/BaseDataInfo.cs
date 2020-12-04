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
        /// 当天最低价位(比较用)
        /// </summary>
        public decimal DayMinValTmp { get; set; }

        /// <summary>
        /// 当天最高价位(比较用)
        /// </summary>
        public decimal DayMaxValTmp { get; set; }

        /// <summary>
        /// 当前点的分型
        /// </summary>
        public PointType PointType { get; set; }

        /// <summary>
        /// 到当前的点时，判断出了前一个点的类型
        /// -1：前一个是低点，1：前一个是高点
        /// </summary>
        public int CheckPoint { get; set; }

        /// <summary>
        /// 当前笔的分型
        /// </summary>
        public PointType PenType { get; set; }

        /// <summary>
        /// 买卖点标识(-3,-2,-1,0,1,2,3)
        /// </summary>
        public int BuySellFlg { get; set; }

        /// <summary>
        /// 日均线价位
        /// </summary>
        public decimal DayAvgVal { get; set; }

        /// <summary>
        /// 5日均线价位
        /// </summary>
        public decimal Day5AvgVal { get; set; }

        /// <summary>
        /// 10日均线价位
        /// </summary>
        public decimal Day10AvgVal { get; set; }

        /// <summary>
        /// 30日均线价位
        /// </summary>
        public decimal Day30AvgVal { get; set; }

        /// <summary>
        /// 上一个比较点的情报
        /// </summary>
        public BaseDataInfo LastChkPoint { get; set; }

        /// <summary>
        /// 当前笔的高点在原始List中的位置
        /// </summary>
        public int PenTopPos { get; set; }

        /// <summary>
        /// 当前笔的低点点在原始List中的位置
        /// </summary>
        public int PenBottomPos { get; set; }
    }
}
