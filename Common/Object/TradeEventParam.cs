using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 交易事件的参数
    /// </summary>
    public class TradeEventParam
    {
        /// <summary>
        /// 当前操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 当前错误信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 当前操作
        /// </summary>
        public CurOpt CurOpt { get; set; }

        /// <summary>
        /// 当前总金额
        /// </summary>
        public decimal TotalMoney { get; set; }

        /// <summary>
        /// 当前市值
        /// </summary>
        public decimal GuPiaoMoney { get; set; }

        /// <summary>
        /// 可以使用的金额
        /// </summary>
        public decimal CanUseMoney { get; set; }

        /// <summary>
        /// 可以提取的金额
        /// </summary>
        public decimal CanGetMoney { get; set; }

        /// <summary>
        /// 历史数据数量
        /// </summary>
        public int HstDataCount { get; set; }
    }
}
