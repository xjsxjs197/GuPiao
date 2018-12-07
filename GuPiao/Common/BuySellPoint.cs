using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuPiao
{
    /// <summary>
    /// 买卖点配置信息
    /// </summary>
    public class BuySellPoint
    {
        /// <summary>
        /// 交易代码
        /// </summary>
        public string StockCd { get; set; }

        /// <summary>
        /// 高位卖点
        /// </summary>
        public float TopSellPoint { get; set; }

        /// <summary>
        /// 低位卖点
        /// </summary>
        public float BottomSellPoint { get; set; }

        /// <summary>
        /// 高位买点
        /// </summary>
        public float TopBuyPoint { get; set; }

        /// <summary>
        /// 低位买点
        /// </summary>
        public float BottomBuyPoint { get; set; }

        /// <summary>
        /// 自动卖的等待时间
        /// </summary>
        public int SellWaitTime { get; set; }

        /// <summary>
        /// 自动买的等待时间
        /// </summary>
        public int BuyWaitTime { get; set; }

        /// <summary>
        /// 犹豫的点（上下浮动的点）
        /// </summary>
        public float WaitPoint { get; set; }
    }
}
