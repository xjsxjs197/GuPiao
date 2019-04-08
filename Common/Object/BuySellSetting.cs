using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 基础配置信息
    /// </summary>
    public class BuySellSetting
    {
        /// <summary>
        /// 取多少天以前的数据(模拟运行)
        /// </summary>
        public int BefDay { get; set; }

        /// <summary>
        /// 设置几个运行线程
        /// </summary>
        public int ThreadCnt { get; set; }

        /// <summary>
        /// 每个线程多少钱
        /// </summary>
        public decimal ThreadMoney { get; set; }

        /// <summary>
        /// 是否倒序读取数据
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// 数据均值长度
        /// </summary>
        public int AvgDataLen { get; set; }

        /// <summary>
        /// 是否需要创业版数据
        /// </summary>
        public bool NeedChuangYe { get; set; }

        /// <summary>
        /// 是否需要融资融券数据
        /// </summary>
        public bool NeedRongZiRongQuan { get; set; }

        /// <summary>
        /// 每秒取多少数据
        /// </summary>
        public int DataCntPerSecond { get; set; }

        /// <summary>
        /// 系统名称以及版本号
        /// </summary>
        public string SystemTitle { get; set; }

        /// <summary>
        /// 趋势图角度达到多少可以买
        /// </summary>
        public int ButStrongth { get; set; }

        /// <summary>
        /// 自动买卖的级别
        /// </summary>
        public string AutoTradeLevel { get; set; }
    }
}
