using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 买卖点临时信息
    /// </summary>
    public class BuySellItem
    {
        /// <summary>
        /// 线程ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string StockCd { get; set; }

        /// <summary>
        /// 买卖状态
        /// </summary>
        public BuySellStatus Status { get; set; }

        /// <summary>
        /// 买点价格
        /// </summary>
        public decimal BuyPrice { get; set; }

        /// <summary>
        /// 买点数量
        /// </summary>
        public int BuyCnt { get; set; }

        /// <summary>
        /// 卖点价格
        /// </summary>
        public decimal SellPrice { get; set; }

        /// <summary>
        /// 卖点数量
        /// </summary>
        public int SellCnt { get; set; }

        /// <summary>
        /// 剩余的金额
        /// </summary>
        public decimal TotalMoney { get; set; }

        /// <summary>
        /// 交易的时间
        /// </summary>
        public string Time { get; set; }
    }
}
