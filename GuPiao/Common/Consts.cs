using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuPiao
{
    /// <summary>
    /// 定数
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// 自动卖的等待时间（秒）
        /// </summary>
        public const int CELL_WAIT_TIME = 5;

        /// <summary>
        /// 自动买的等待时间（秒）
        /// </summary>
        public const int BUY_WAIT_TIME = 5;

        /// <summary>
        /// 不作为变化的判断范围
        /// </summary>
        public const decimal LIMIT_VAL = (decimal)1.005;

        /// <summary>
        /// 查找趋势时连续的天数
        /// </summary>
        public const int QUSHI_CONTINUE_DAYS = 3;

        /// <summary>
        /// 顶分型
        /// </summary>
        public const int TOP_TYPE = int.MaxValue;

        /// <summary>
        /// 底分型
        /// </summary>
        public const int BOTTOM_TYPE = int.MinValue;

        /// <summary>
        /// 数据结束，没有出现分型
        /// </summary>
        public const int NONE_TYPE = 9999999;

        /// <summary>
        /// 卖点判断基数
        /// </summary>
        public const decimal SELL_VAL = (decimal)1.01;
    }
}
